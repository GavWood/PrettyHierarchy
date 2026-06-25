// PrettyHierarchyDrawer.cs
using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static partial class PrettyHierarchy
{
    private const float IconSlotSize = 16f;

    private static HierarchyIconSettings settings;
    private static GUIStyle separatorLabelStyle;

    static PrettyHierarchy()
    {
#if UNITY_6000_4_OR_NEWER
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= HierarchyItemOnGUI;
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += HierarchyItemOnGUI;
#else
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyItemOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
#endif

        EditorApplication.projectChanged -= OnProjectChanged;
        EditorApplication.projectChanged += OnProjectChanged;

        LoadSettings();

        EditorApplication.RepaintHierarchyWindow();
    }

    private static void OnProjectChanged()
    {
        LoadSettings();
        EditorApplication.RepaintHierarchyWindow();
    }

    private static void LoadSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:HierarchyIconSettings");

        if (guids.Length == 0)
        {
            CreateSettingsAsset();
            return;
        }

        if (guids.Length > 1)
            Debug.LogWarning($"Multiple Pretty Hierarchy settings assets found ({guids.Length}). Using the first one.");

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        settings = AssetDatabase.LoadAssetAtPath<HierarchyIconSettings>(path);
    }

    private static void CreateSettingsAsset()
    {
        const string folderPath = "Assets/Editor";
        const string assetPath = "Assets/Editor/PrettyHierarchySettings.asset";

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "Editor");

        settings = ScriptableObject.CreateInstance<HierarchyIconSettings>();

        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created Pretty Hierarchy settings automatically at '{assetPath}'.");
    }

#if UNITY_6000_4_OR_NEWER
    private static void HierarchyItemOnGUI(EntityId entityId, Rect selectionRect)
    {
        GameObject obj = EditorUtility.EntityIdToObject(entityId) as GameObject;
        DrawHierarchyItem(obj, selectionRect);
    }
#else
    private static void HierarchyItemOnGUI(int instanceID, Rect selectionRect)
    {
#pragma warning disable CS0618
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore CS0618

        DrawHierarchyItem(obj, selectionRect);
    }
#endif

    private static void DrawHierarchyItem(GameObject obj, Rect selectionRect)
    {
        if (settings == null || obj == null)
            return;

        HierarchyIconSettings.ObjectIconEntry objectEntry = settings.GetObjectEntry(obj);
        MonoScript script = GetFirstNonUnityScript(obj);

        Rect iconSlotRect = new Rect(
            selectionRect.xMin,
            selectionRect.y,
            IconSlotSize,
            selectionRect.height);

        if (objectEntry != null && objectEntry.showColourBar)
            DrawColourBar(selectionRect, obj, objectEntry);

        Texture2D icon = objectEntry != null
            ? objectEntry.GetFinalIcon()
            : GetComponentFallbackIcon(obj);

        if (objectEntry != null && objectEntry.showColourIcon)
        {
            DrawColourIconSlot(iconSlotRect, objectEntry.separatorColor);

            if (icon != null)
                DrawIconTexture(iconSlotRect, icon);
        }
        else if (icon != null)
        {
            DrawIcon(iconSlotRect, icon);
        }
        else
        {
            DrawAddIconHint(iconSlotRect);
        }

        HandleRowClick(selectionRect, iconSlotRect, obj, script);
    }

    private static void DrawColourBar(
        Rect selectionRect,
        GameObject obj,
        HierarchyIconSettings.ObjectIconEntry entry)
    {
        EditorGUI.DrawRect(selectionRect, entry.separatorColor);

        separatorLabelStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            clipping = TextClipping.Clip,
            normal = new GUIStyleState
            {
                textColor = Color.white
            }
        };

        EditorGUI.LabelField(selectionRect, obj.name, separatorLabelStyle);
    }

    private static void DrawColourIconSlot(Rect iconSlotRect, Color colour)
    {
        EditorGUI.DrawRect(iconSlotRect, colour);

        EditorGUI.DrawRect(
            iconSlotRect,
            EditorGUIUtility.isProSkin
                ? new Color(0f, 0f, 0f, 0.22f)
                : new Color(0f, 0f, 0f, 0.14f));
    }

    private static Texture2D GetComponentFallbackIcon(GameObject obj)
    {
        var entries = settings.Entries;

        if (entries == null || entries.Count == 0)
            return null;

        for (int i = 0; i < entries.Count; i++)
        {
            HierarchyIconSettings.ScriptIconEntry entry = entries[i];

            if (entry == null)
                continue;

            Type type = entry.GetComponentType();

            if (type == null)
                continue;

            if (obj.GetComponent(type) == null)
                continue;

            return entry.GetFinalIcon();
        }

        return null;
    }

    private static MonoScript GetFirstNonUnityScript(GameObject obj)
    {
        MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
                continue;

            MonoScript script = MonoScript.FromMonoBehaviour(behaviour);

            if (script == null)
                continue;

            string path = AssetDatabase.GetAssetPath(script);

            if (string.IsNullOrEmpty(path))
                continue;

            if (IsUnityScriptPath(path))
                continue;

            return script;
        }

        return null;
    }

    private static bool IsUnityScriptPath(string path)
    {
        if (path.StartsWith("Assets/", StringComparison.Ordinal))
            return false;

        if (path.StartsWith("Packages/com.unity.", StringComparison.Ordinal))
            return true;

        if (path.StartsWith("Packages/com.unity3d.", StringComparison.Ordinal))
            return true;

        return false;
    }

    private static void DrawIcon(Rect slotRect, Texture2D icon)
    {
        Rect coverRect = new Rect(
            slotRect.x,
            slotRect.y,
            IconSlotSize,
            slotRect.height);

        Color coverColour = EditorGUIUtility.isProSkin
            ? new Color(0.219f, 0.219f, 0.219f, 1f)
            : new Color(0.76f, 0.76f, 0.76f, 1f);

        EditorGUI.DrawRect(coverRect, coverColour);
        DrawIconTexture(slotRect, icon);
    }

    private static void DrawIconTexture(Rect slotRect, Texture2D icon)
    {
        Rect iconRect = new Rect(
            slotRect.x + 1f,
            slotRect.y + 1f,
            14f,
            14f);

        Color previous = GUI.color;

        GUI.color = EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.78f)
            : new Color(1f, 1f, 1f, 0.88f);

        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);

        GUI.color = previous;
    }

    private static void DrawAddIconHint(Rect slotRect)
    {
        Event current = Event.current;

        if (!slotRect.Contains(current.mousePosition))
            return;

        Rect iconRect = new Rect(
            slotRect.x + 2f,
            slotRect.y + 2f,
            12f,
            12f);

        Color previous = GUI.color;

        GUI.color = new Color(1f, 1f, 1f, 0.35f);
        GUI.Label(iconRect, "+");
        GUI.color = previous;
    }
}