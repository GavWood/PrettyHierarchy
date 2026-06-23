using UnityEditor;
using UnityEngine;

public static partial class PrettyHierarchy
{
    private sealed class BuiltinIconDropdown : EditorWindow
{
    private HierarchyIconSettings.BuiltinIconType selectedType;
    private System.Action<HierarchyIconSettings.BuiltinIconType> onSelected;

    public static void Open(
        Rect activatorRect,
        HierarchyIconSettings.BuiltinIconType currentType,
        System.Action<HierarchyIconSettings.BuiltinIconType> selectedCallback)
    {
        BuiltinIconDropdown window = CreateInstance<BuiltinIconDropdown>();

        window.selectedType = currentType;
        window.onSelected = selectedCallback;

        Rect screenRect = new(
            GUIUtility.GUIToScreenPoint(activatorRect.position),
            activatorRect.size);

        window.ShowAsDropDown(
            screenRect,
            new Vector2(220f, BuiltinTypes.Length * 22f + 4f));
    }

    private void OnGUI()
    {
        for (int i = 0; i < BuiltinTypes.Length; i++)
            DrawRow(BuiltinTypes[i]);
    }

    private void DrawRow(HierarchyIconSettings.BuiltinIconType type)
    {
        Rect rowRect = GUILayoutUtility.GetRect(
            1f,
            22f,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(22f));

        if (Event.current.type == EventType.Repaint)
        {
            bool isSelected = selectedType == type;

            if (isSelected)
                EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 0.65f));
            else if (rowRect.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.08f));
        }

        Texture2D icon = HierarchyIconSettings.GetBuiltinIcon(type);

        Rect iconRect = new(
            rowRect.x + 6f,
            rowRect.y + 3f,
            16f,
            16f);

        Rect labelRect = new(
            iconRect.xMax + 8f,
            rowRect.y,
            rowRect.width - iconRect.width - 20f,
            rowRect.height);

        if (type == HierarchyIconSettings.BuiltinIconType.None)
        {
            GUIStyle noneStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            EditorGUI.LabelField(iconRect, "×", noneStyle);
        }
        else if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        }

        EditorGUI.LabelField(labelRect, type.ToString());

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rowRect.Contains(Event.current.mousePosition))
        {
            onSelected?.Invoke(type);
            Event.current.Use();
            Close();
        }
    }
}

    private readonly struct SeparatorPreset
    {
        public readonly string Label;
        public readonly Color Colour;
        public readonly bool ClearsSeparator;

        public SeparatorPreset(string label, Color colour, bool clearsSeparator = false)
        {
            Label = label;
            Colour = colour;
            ClearsSeparator = clearsSeparator;
        }
    }

    private static readonly SeparatorPreset[] SeparatorPresets =
    {
        new("None", Color.clear, true),
        new("Debug", new Color(0.15f, 0.15f, 0.15f, 1f)),
        new("Networking", new Color(0.20f, 0.32f, 0.65f, 1f)),
        new("Game Flow", new Color(0.42f, 0.24f, 0.58f, 1f)),
        new("Scene", new Color(0.20f, 0.48f, 0.32f, 1f)),
        new("Config", new Color(0.65f, 0.35f, 0.15f, 1f)),
        new("Permissions", new Color(0.58f, 0.20f, 0.20f, 1f)),
        new("UI", new Color(0.15f, 0.45f, 0.55f, 1f))
    };

    private static readonly HierarchyIconSettings.BuiltinIconType[] BuiltinTypes =
    {
        HierarchyIconSettings.BuiltinIconType.None,
        HierarchyIconSettings.BuiltinIconType.Android,
        HierarchyIconSettings.BuiltinIconType.PC,
        HierarchyIconSettings.BuiltinIconType.Editor,
        HierarchyIconSettings.BuiltinIconType.Prefab,
        HierarchyIconSettings.BuiltinIconType.Folder,
        HierarchyIconSettings.BuiltinIconType.Script,
        HierarchyIconSettings.BuiltinIconType.Camera,
        HierarchyIconSettings.BuiltinIconType.Light,
        HierarchyIconSettings.BuiltinIconType.Audio,
        HierarchyIconSettings.BuiltinIconType.Anchor
    };

    private static GUIContent[] builtinIconContents;

    private static void HandleRowClick(
        Rect rowRect,
        Rect iconRect,
        GameObject obj,
        MonoScript script)
    {
        Event current = Event.current;

        Rect iconClickRect = new(
            iconRect.x,
            iconRect.y,
            iconRect.width,
            iconRect.height);

        if (current.type == EventType.ContextClick &&
            iconClickRect.Contains(current.mousePosition))
        {
            Selection.activeGameObject = obj;

            PrettyHierarchyDialog.Open(
                settings,
                obj,
                script,
                SeparatorPresets);

            current.Use();
            return;
        }

        if (current.type == EventType.MouseDown &&
            current.button == 0 &&
            current.clickCount == 2 &&
            iconRect.Contains(current.mousePosition))
        {
            OpenScript(script);
            current.Use();
        }
    }

    private static void OpenScript(MonoScript script)
    {
        if (script == null)
            return;

        AssetDatabase.OpenAsset(script);
    }

    private static GUIContent[] GetBuiltinIconContents()
    {
        if (builtinIconContents != null)
            return builtinIconContents;

        builtinIconContents = new GUIContent[BuiltinTypes.Length];

        for (int i = 0; i < BuiltinTypes.Length; i++)
        {
            HierarchyIconSettings.BuiltinIconType type = BuiltinTypes[i];

            builtinIconContents[i] = new GUIContent(
                type.ToString(),
                HierarchyIconSettings.GetBuiltinIcon(type));
        }

        return builtinIconContents;
    }

    private sealed class PrettyHierarchyDialog : EditorWindow
    {
        private HierarchyIconSettings targetSettings;
        private GameObject targetObject;
        private MonoScript targetScript;
        private HierarchyIconSettings.ObjectIconEntry targetEntry;
        private SeparatorPreset[] presets;

        public static void Open(
            HierarchyIconSettings settings,
            GameObject obj,
            MonoScript script,
            SeparatorPreset[] separatorPresets)
        {
            PrettyHierarchyDialog window = GetWindow<PrettyHierarchyDialog>(
                true,
                "Pretty Hierarchy",
                true);

            window.minSize = new Vector2(360f, 430f);

            window.SetTarget(
                settings,
                obj,
                script,
                separatorPresets);

            window.Repaint();
            window.Focus();
        }

        private void SetTarget(
            HierarchyIconSettings newSettings,
            GameObject newObject,
            MonoScript newScript,
            SeparatorPreset[] newPresets)
        {
            targetSettings = newSettings;
            targetObject = newObject;
            targetScript = newScript;
            presets = newPresets ?? SeparatorPresets;
            targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

            Repaint();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null)
                return;

            if (targetSettings == null)
                return;

            MonoScript script = GetFirstNonUnityScript(Selection.activeGameObject);

            SetTarget(
                targetSettings,
                Selection.activeGameObject,
                script,
                SeparatorPresets);

            Repaint();
        }

        private void OnGUI()
        {
            if (targetSettings == null || targetObject == null || targetEntry == null)
            {
                Close();
                return;
            }

            EditorGUILayout.Space(8f);

            EditorGUILayout.HelpBox(
                "Context click to open this menu.\nDouble click the icon area to open the first user script.",
                MessageType.Info);

            EditorGUILayout.Space(6f);

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Object", targetObject, typeof(GameObject), true);

            EditorGUILayout.LabelField("Icon", EditorStyles.boldLabel);

            DrawCustomTextureField();
            DrawBuiltinIconField();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Separator", EditorStyles.boldLabel);

            DrawSeparatorFields();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preset Colours", EditorStyles.boldLabel);
            DrawPresetButtons();

            GUILayout.FlexibleSpace();

            EditorGUILayout.Space(8f);
            DrawClearButtons();
            EditorGUILayout.Space(8f);
        }

        private void DrawCustomTextureField()
        {
            EditorGUI.BeginChangeCheck();

            Texture2D newIcon = EditorGUILayout.ObjectField(
                "Custom Texture",
                targetEntry.icon,
                typeof(Texture2D),
                false) as Texture2D;

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(targetSettings, "Set Pretty Hierarchy Custom Icon");

            targetEntry.icon = newIcon;

            if (newIcon != null)
            {
                targetEntry.builtinIcon = HierarchyIconSettings.BuiltinIconType.None;
                targetEntry.isSeparator = true;
                targetEntry.showSeparatorIcon = true;
            }

            MarkDirty();
        }

        private void DrawBuiltinIconField()
        {
            Rect controlRect = EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight);

            Rect labelRect = new(
                controlRect.x,
                controlRect.y,
                EditorGUIUtility.labelWidth,
                controlRect.height);

            Rect dropdownRect = new(
                controlRect.x + EditorGUIUtility.labelWidth,
                controlRect.y,
                controlRect.width - EditorGUIUtility.labelWidth,
                controlRect.height);

            EditorGUI.LabelField(labelRect, "Built-in Icon");

            Texture2D selectedIcon =
                HierarchyIconSettings.GetBuiltinIcon(targetEntry.builtinIcon);

            GUIContent buttonContent = new(
                targetEntry.builtinIcon.ToString(),
                selectedIcon);

            if (GUI.Button(dropdownRect, buttonContent, EditorStyles.popup))
            {
                BuiltinIconDropdown.Open(
                    dropdownRect,
                    targetEntry.builtinIcon,
                    selectedType =>
                    {
                        Undo.RecordObject(
                            targetSettings,
                            "Set Pretty Hierarchy Built-in Icon");

                        targetEntry.builtinIcon = selectedType;
                        targetEntry.icon = null;

                        if (selectedType != HierarchyIconSettings.BuiltinIconType.None)
                        {
                            targetEntry.isSeparator = true;
                            targetEntry.showSeparatorIcon = true;
                        }

                        MarkDirty();
                    });
            }
        }

        private void DrawSeparatorFields()
        {
            EditorGUI.BeginChangeCheck();
            bool showIcon = EditorGUILayout.Toggle("Show Icon", targetEntry.showSeparatorIcon);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSettings, "Toggle Pretty Hierarchy Separator Icon");
                targetEntry.showSeparatorIcon = showIcon;
                MarkDirty();
            }

            EditorGUI.BeginChangeCheck();
            Color colour = EditorGUILayout.ColorField("Colour", targetEntry.separatorColor);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSettings, "Set Pretty Hierarchy Separator Colour");

                targetEntry.isSeparator = true;
                targetEntry.separatorColor = colour;

                MarkDirty();
            }
        }

        private void DrawPresetButtons()
        {
            presets ??= SeparatorPresets;

            const int columns = 2;

            for (int i = 0; i < presets.Length; i++)
            {
                if (i % columns == 0)
                    EditorGUILayout.BeginHorizontal();

                DrawPresetButton(presets[i]);

                if (i % columns == columns - 1)
                    EditorGUILayout.EndHorizontal();
            }

            if (presets.Length % columns != 0)
                EditorGUILayout.EndHorizontal();
        }

        private void DrawPresetButton(SeparatorPreset preset)
        {
            Rect rect = GUILayoutUtility.GetRect(
                1f,
                24f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(24f));

            if (GUI.Button(rect, GUIContent.none, EditorStyles.miniButton))
            {
                if (preset.ClearsSeparator)
                {
                    Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Separator");

                    targetSettings.ClearObjectSeparator(targetObject);
                    targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                    MarkDirty();
                    return;
                }

                Undo.RecordObject(
                    targetSettings,
                    $"Apply {preset.Label} Pretty Hierarchy Colour");

                targetEntry.isSeparator = true;
                targetEntry.separatorColor = preset.Colour;

                MarkDirty();
            }

            Rect colourRect = new(
                rect.x + 6f,
                rect.y + 3f,
                12f,
                12f);

            if (preset.ClearsSeparator)
            {
                EditorGUI.DrawRect(colourRect, Color.black);

                GUIStyle crossStyle = new(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                EditorGUI.LabelField(
                    colourRect,
                    "×",
                    crossStyle);
            }
            else
            {
                EditorGUI.DrawRect(colourRect, Color.black);

                EditorGUI.DrawRect(
                    new Rect(
                        colourRect.x + 1f,
                        colourRect.y + 1f,
                        10f,
                        10f),
                    preset.Colour);
            }

            GUIStyle labelStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal
            };

            Rect labelRect = new(
                rect.x,
                rect.y - 3f,
                rect.width,
                rect.height);

            EditorGUI.LabelField(
                labelRect,
                preset.Label,
                labelStyle);
        }

        private void DrawClearButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Icon"))
            {
                Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Object Icon");

                targetSettings.ClearObjectIcon(targetObject);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                MarkDirty();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Everything"))
            {
                Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Entry");

                targetSettings.RemoveObjectEntry(targetObject);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                MarkDirty();
            }
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(targetSettings);
            EditorApplication.RepaintHierarchyWindow();
            Repaint();
        }
    }
}