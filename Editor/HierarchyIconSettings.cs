// HierarchyIconSettings.cs
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Pretty Hierarchy/Settings")]
public sealed class HierarchyIconSettings : ScriptableObject
{
    public enum BuiltinIconType
    {
        None,
        Android,
        PC,
        Editor,
        Prefab,
        Folder,
        Script,
        Camera,
        Light,
        Audio,
        Anchor
    }

        public static Texture2D GetBuiltinIcon(BuiltinIconType type)
    {
        return type switch
        {
            BuiltinIconType.Android => EditorGUIUtility.IconContent("BuildSettings.Android").image as Texture2D,
            BuiltinIconType.PC => EditorGUIUtility.IconContent("BuildSettings.Standalone").image as Texture2D,
            BuiltinIconType.Editor => EditorGUIUtility.IconContent("BuildSettings.Editor").image as Texture2D,
            BuiltinIconType.Prefab => EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D,
            BuiltinIconType.Folder => EditorGUIUtility.IconContent("Folder Icon").image as Texture2D,
            BuiltinIconType.Script => EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D,
            BuiltinIconType.Camera => EditorGUIUtility.IconContent("Camera Icon").image as Texture2D,
            BuiltinIconType.Light => EditorGUIUtility.IconContent("Light Icon").image as Texture2D,
            BuiltinIconType.Audio => EditorGUIUtility.IconContent("AudioSource Icon").image as Texture2D,
            BuiltinIconType.Anchor => EditorGUIUtility.IconContent("d_SceneViewOrtho").image as Texture2D,
            _ => null
        };
    }

    [Serializable]
    public sealed class ObjectIconEntry
    {
        public string globalObjectId;

        [Header("Icon")]
        public Texture2D icon;
        public BuiltinIconType builtinIcon;

        [Header("Separator")]
        public bool isSeparator;
        public Color separatorColor = new(0.35f, 0.35f, 0.35f, 1f);
        public bool showSeparatorIcon;

        public Texture2D GetFinalIcon()
        {
            if (icon != null)
                return icon;

            return GetBuiltinIcon(builtinIcon);
        }

        public bool IsEmpty()
        {
            return icon == null &&
                   builtinIcon == BuiltinIconType.None &&
                   !isSeparator;
        }
    }

    [Serializable]
    public sealed class ScriptIconEntry
    {
        [Header("Component Type")]
        public MonoScript script;

        [Tooltip("Optional fallback for package/runtime types. Example: Unity.Netcode.NetworkManager, Unity.Netcode.Runtime")]
        public string typeName;

        [Header("Single Icon")]
        public Texture2D icon;
        public BuiltinIconType builtinIcon;

        public Type GetComponentType()
        {
            Type type = GetTypeFromScript();

            if (type == null)
                type = GetTypeFromName();

            if (type == null)
                return null;

            if (!typeof(Component).IsAssignableFrom(type))
                return null;

            return type;
        }

        public Texture2D GetFinalIcon()
        {
            if (icon != null)
                return icon;

            return GetBuiltinIcon(builtinIcon);
        }

        private Type GetTypeFromScript()
        {
            if (script == null)
                return null;

            return script.GetClass();
        }

        private Type GetTypeFromName()
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            Type type = Type.GetType(typeName);

            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }

            return null;
        }
    }

    [SerializeField] private List<ObjectIconEntry> objectEntries = new();
    [SerializeField] private List<ScriptIconEntry> entries = new();

    public IReadOnlyList<ObjectIconEntry> ObjectEntries => objectEntries;
    public IReadOnlyList<ScriptIconEntry> Entries => entries;

    public ObjectIconEntry GetOrCreateObjectEntry(GameObject obj)
    {
        string key = GetObjectKey(obj);
        ObjectIconEntry existing = GetObjectEntry(key);

        if (existing != null)
            return existing;

        ObjectIconEntry created = new()
        {
            globalObjectId = key,
            builtinIcon = BuiltinIconType.None,
            separatorColor = new Color(0.35f, 0.35f, 0.35f, 1f)
        };

        objectEntries.Add(created);
        EditorUtility.SetDirty(this);

        return created;
    }

    public ObjectIconEntry GetObjectEntry(GameObject obj)
    {
        return GetObjectEntry(GetObjectKey(obj));
    }

    public void ClearObjectIcon(GameObject obj)
    {
        ObjectIconEntry entry = GetObjectEntry(obj);

        if (entry == null)
            return;

        entry.icon = null;
        entry.builtinIcon = BuiltinIconType.None;

        RemoveIfEmpty(entry);
    }

    public void ClearObjectSeparator(GameObject obj)
    {
        ObjectIconEntry entry = GetObjectEntry(obj);

        if (entry == null)
            return;

        entry.isSeparator = false;
        entry.showSeparatorIcon = false;

        RemoveIfEmpty(entry);
    }

    public void RemoveObjectEntry(GameObject obj)
    {
        if (obj == null)
            return;

        string globalKey = GetObjectKey(obj);
        string scenePathKey = GetScenePathKey(obj);
        string nameKey = obj.name;

        objectEntries.RemoveAll(entry =>
            entry == null ||
            entry.globalObjectId == globalKey ||
            entry.globalObjectId == scenePathKey ||
            entry.globalObjectId == nameKey ||
            string.IsNullOrEmpty(entry.globalObjectId));

        EditorUtility.SetDirty(this);
        EditorApplication.RepaintHierarchyWindow();
    }

    private void RemoveIfEmpty(ObjectIconEntry entry)
    {
        if (entry == null)
            return;

        if (!entry.IsEmpty())
            return;

        objectEntries.Remove(entry);
        EditorUtility.SetDirty(this);
        EditorApplication.RepaintHierarchyWindow();
    }

    private ObjectIconEntry GetObjectEntry(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        for (int i = 0; i < objectEntries.Count; i++)
        {
            ObjectIconEntry entry = objectEntries[i];

            if (entry == null)
                continue;

            if (entry.globalObjectId == key)
                return entry;
        }

        return null;
    }

    private static string GetObjectKey(GameObject obj)
    {
        if (obj == null)
            return string.Empty;

        GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
        return globalObjectId.ToString();
    }

    private static string GetScenePathKey(GameObject obj)
    {
        if (obj == null)
            return string.Empty;

        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return obj.scene.path + "::" + path;
    }

    private void OnValidate()
    {
        EditorApplication.RepaintHierarchyWindow();
    }
}