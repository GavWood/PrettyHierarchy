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

    [Serializable]
    public sealed class ColourPreset
    {
        public string presetId;
        public string displayName = "New Preset";
        public Color colour = new(0.35f, 0.35f, 0.35f, 1f);
        public Texture2D icon;
        public BuiltinIconType builtinIcon;
        public bool showIcon = true;

        public Texture2D GetFinalIcon()
        {
            if (icon != null)
                return icon;

            return GetBuiltinIcon(builtinIcon);
        }
    }

    [Serializable]
    public sealed class ObjectIconEntry
    {
        public string globalObjectId;
        public string hierarchyPath;
        public string presetId;

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

    [SerializeField] private List<ColourPreset> colourPresets = new();
    [SerializeField] private List<ObjectIconEntry> objectEntries = new();
    [SerializeField] private List<ScriptIconEntry> entries = new();

    public IReadOnlyList<ColourPreset> ColourPresets => colourPresets;
    public IReadOnlyList<ObjectIconEntry> ObjectEntries => objectEntries;
    public IReadOnlyList<ScriptIconEntry> Entries => entries;

    private void OnEnable()
    {
        EnsureDefaultColourPresets();
        EnsurePresetIds();
    }

    private void OnValidate()
    {
        EnsureDefaultColourPresets();
        EnsurePresetIds();
        RefreshAllPresetLinkedObjects();

        EditorApplication.RepaintHierarchyWindow();
    }

    private void EnsureDefaultColourPresets()
    {
        if (colourPresets != null && colourPresets.Count > 0)
            return;

        colourPresets = CreateDefaultColourPresets();
        EditorUtility.SetDirty(this);
    }

    private void EnsurePresetIds()
    {
        if (colourPresets == null)
            colourPresets = new List<ColourPreset>();

        bool changed = false;
        HashSet<string> usedIds = new();

        for (int i = 0; i < colourPresets.Count; i++)
        {
            ColourPreset preset = colourPresets[i];

            if (preset == null)
                continue;

            if (string.IsNullOrWhiteSpace(preset.presetId) || usedIds.Contains(preset.presetId))
            {
                preset.presetId = Guid.NewGuid().ToString("N");
                changed = true;
            }

            usedIds.Add(preset.presetId);
        }

        if (changed)
            EditorUtility.SetDirty(this);
    }

    private static List<ColourPreset> CreateDefaultColourPresets()
    {
        return new List<ColourPreset>
        {
            CreatePreset("Debug", new Color(0.15f, 0.15f, 0.15f, 1f), BuiltinIconType.Editor),
            CreatePreset("Networking", new Color(0.20f, 0.32f, 0.65f, 1f), BuiltinIconType.Anchor),
            CreatePreset("Game Flow", new Color(0.42f, 0.24f, 0.58f, 1f), BuiltinIconType.Prefab),
            CreatePreset("Scene", new Color(0.20f, 0.48f, 0.32f, 1f), BuiltinIconType.Folder),
            CreatePreset("Config", new Color(0.65f, 0.35f, 0.15f, 1f), BuiltinIconType.Script),
            CreatePreset("Permissions", new Color(0.58f, 0.20f, 0.20f, 1f), BuiltinIconType.Android),
            CreatePreset("UI", new Color(0.15f, 0.45f, 0.55f, 1f), BuiltinIconType.Camera)
        };
    }

    private static ColourPreset CreatePreset(string displayName, Color colour, BuiltinIconType builtinIcon)
    {
        return new ColourPreset
        {
            presetId = Guid.NewGuid().ToString("N"),
            displayName = displayName,
            colour = colour,
            builtinIcon = builtinIcon,
            showIcon = true
        };
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

    public ObjectIconEntry GetOrCreateObjectEntry(GameObject obj)
    {
        if (obj == null)
            return null;

        string globalKey = GetGlobalObjectKey(obj);
        string pathKey = GetHierarchyPath(obj);

        ObjectIconEntry existing = GetObjectEntry(obj);

        if (existing != null)
        {
            existing.globalObjectId = globalKey;
            existing.hierarchyPath = pathKey;

            EditorUtility.SetDirty(this);
            return existing;
        }

        ObjectIconEntry created = new()
        {
            globalObjectId = globalKey,
            hierarchyPath = pathKey,
            builtinIcon = BuiltinIconType.None,
            separatorColor = new Color(0.35f, 0.35f, 0.35f, 1f)
        };

        objectEntries.Add(created);
        EditorUtility.SetDirty(this);

        return created;
    }

    public ObjectIconEntry GetObjectEntry(GameObject obj)
    {
        if (obj == null)
            return null;

        string globalKey = GetGlobalObjectKey(obj);
        string pathKey = GetHierarchyPath(obj);

        ObjectIconEntry globalMatch = GetObjectEntryByGlobalId(globalKey);

        if (globalMatch != null)
            return globalMatch;

        return GetObjectEntryByPath(pathKey);
    }

    public void ApplyPreset(GameObject obj, ColourPreset preset)
    {
        if (obj == null || preset == null)
            return;

        EnsurePresetIds();

        ObjectIconEntry entry = GetOrCreateObjectEntry(obj);

        if (entry == null)
            return;

        entry.presetId = preset.presetId;
        ApplyPresetToEntry(entry, preset);

        EditorUtility.SetDirty(this);
        EditorApplication.RepaintHierarchyWindow();
    }

    public void RefreshObjectsUsingPreset(ColourPreset preset)
    {
        if (preset == null || string.IsNullOrEmpty(preset.presetId))
            return;

        for (int i = 0; i < objectEntries.Count; i++)
        {
            ObjectIconEntry entry = objectEntries[i];

            if (entry == null)
                continue;

            if (entry.presetId != preset.presetId)
                continue;

            ApplyPresetToEntry(entry, preset);
        }

        EditorUtility.SetDirty(this);
        EditorApplication.RepaintHierarchyWindow();
    }

    public void RefreshAllPresetLinkedObjects()
    {
        if (colourPresets == null || objectEntries == null)
            return;

        for (int i = 0; i < colourPresets.Count; i++)
            RefreshObjectsUsingPreset(colourPresets[i]);
    }

    private static void ApplyPresetToEntry(ObjectIconEntry entry, ColourPreset preset)
    {
        entry.isSeparator = true;
        entry.separatorColor = preset.colour;
        entry.showSeparatorIcon = preset.showIcon;
        entry.icon = preset.icon;
        entry.builtinIcon = preset.icon != null ? BuiltinIconType.None : preset.builtinIcon;
    }

    public void ClearObjectIcon(GameObject obj)
    {
        ObjectIconEntry entry = GetObjectEntry(obj);

        if (entry == null)
            return;

        entry.presetId = string.Empty;
        entry.icon = null;
        entry.builtinIcon = BuiltinIconType.None;

        RemoveIfEmpty(entry);
    }

    public void ClearObjectSeparator(GameObject obj)
    {
        ObjectIconEntry entry = GetObjectEntry(obj);

        if (entry == null)
            return;

        entry.presetId = string.Empty;
        entry.isSeparator = false;
        entry.showSeparatorIcon = false;

        RemoveIfEmpty(entry);
    }

    public void RemoveObjectEntry(GameObject obj)
    {
        if (obj == null)
            return;

        string globalKey = GetGlobalObjectKey(obj);
        string pathKey = GetHierarchyPath(obj);

        objectEntries.RemoveAll(entry =>
            entry == null ||
            string.IsNullOrEmpty(entry.globalObjectId) && string.IsNullOrEmpty(entry.hierarchyPath) ||
            entry.globalObjectId == globalKey ||
            EntryCanFallbackToPath(entry) && entry.hierarchyPath == pathKey);

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

    private ObjectIconEntry GetObjectEntryByGlobalId(string globalKey)
    {
        if (string.IsNullOrEmpty(globalKey))
            return null;

        for (int i = 0; i < objectEntries.Count; i++)
        {
            ObjectIconEntry entry = objectEntries[i];

            if (entry == null)
                continue;

            if (entry.globalObjectId == globalKey)
                return entry;
        }

        return null;
    }

    private ObjectIconEntry GetObjectEntryByPath(string pathKey)
    {
        if (string.IsNullOrEmpty(pathKey))
            return null;

        for (int i = 0; i < objectEntries.Count; i++)
        {
            ObjectIconEntry entry = objectEntries[i];

            if (entry == null)
                continue;

            if (entry.hierarchyPath == pathKey)
                return entry;
        }

        return null;
    }

    private bool EntryCanFallbackToPath(ObjectIconEntry entry)
    {
        if (entry == null)
            return false;

        if (string.IsNullOrEmpty(entry.hierarchyPath))
            return false;

        if (string.IsNullOrEmpty(entry.globalObjectId))
            return true;

        return !GlobalObjectIdResolves(entry.globalObjectId);
    }

    private static bool GlobalObjectIdResolves(string globalObjectIdText)
    {
        if (string.IsNullOrEmpty(globalObjectIdText))
            return false;

        if (!GlobalObjectId.TryParse(globalObjectIdText, out GlobalObjectId globalObjectId))
            return false;

        return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) != null;
    }

    private static string GetGlobalObjectKey(GameObject obj)
    {
        if (obj == null)
            return string.Empty;

        GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
        return globalObjectId.ToString();
    }

    private static string GetHierarchyPath(GameObject obj)
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

        return path;
    }
}