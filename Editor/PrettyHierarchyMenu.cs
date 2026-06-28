// PrettyHierarchy.ContextMenu.cs
using System.Reflection;
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
                new Vector2(260f, BuiltinTypes.Length * 24f + 8f));
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
                24f,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(24f));

            if (Event.current.type == EventType.Repaint)
            {
                bool isSelected = selectedType == type;

                if (isSelected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.90f, 0.65f));
                else if (rowRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.08f));
            }

            Texture2D icon = HierarchyIconSettings.GetBuiltinIcon(type);

            Rect iconRect = new(rowRect.x + 8f, rowRect.y + 4f, 16f, 16f);
            Rect labelRect = new(iconRect.xMax + 10f, rowRect.y, rowRect.width - iconRect.width - 24f, rowRect.height);

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

    private static void HandleRowClick(Rect rowRect, Rect iconRect, GameObject obj, MonoScript script)
    {
        Event current = Event.current;

        if (current.type == EventType.ContextClick && iconRect.Contains(current.mousePosition))
        {
            Selection.activeGameObject = obj;
            PrettyHierarchyDialog.Open(settings, obj, script);
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
        private const float WindowWidth = 620f;
        private const float HeaderHeight = 180f;
        private const float PresetRowHeight = 74f;
        private const float PreviewWidth = 70f;
        private const float DeleteButtonWidth = 24f;
        private const float FooterHeight = 48f;
        private const float MaxWindowHeight = 720f;

        private const float ColumnAWidth = 180f;
        private const float ColumnBWidth = 180f;
        private const float ColumnCWidth = 110f;
        private const float ColumnDWidth = 72f;
        private const float ColumnGap = 4f;

        private HierarchyIconSettings targetSettings;
        private GameObject targetObject;
        private MonoScript targetScript;
        private HierarchyIconSettings.ObjectIconEntry targetEntry;
        private Vector2 scroll;

        public static void Open(HierarchyIconSettings settings, GameObject obj, MonoScript script)
        {
            PrettyHierarchyDialog window = CreateInstance<PrettyHierarchyDialog>();

            window.titleContent = new GUIContent("Pretty Hierarchy");

            float windowHeight = CalculateWindowHeight(settings);

            window.minSize = new Vector2(WindowWidth, windowHeight);
            window.maxSize = new Vector2(WindowWidth, windowHeight);
            window.position = new Rect(360f, 80f, WindowWidth, windowHeight);

            window.SetTarget(settings, obj, script);

            window.ShowUtility();
            window.Focus();
        }

        private static float CalculateWindowHeight(HierarchyIconSettings settings)
        {
            int presetCount = settings != null ? settings.ColourPresets.Count : 0;

            return Mathf.Clamp(
                HeaderHeight + FooterHeight + 32f + (presetCount * (PresetRowHeight + 6f)),
                400f,
                MaxWindowHeight);
        }

        private void SetTarget(HierarchyIconSettings newSettings, GameObject newObject, MonoScript newScript)
        {
            targetSettings = newSettings;
            targetObject = newObject;
            targetScript = newScript;
            targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

            ResizeToContent();
            Repaint();
        }

        private void ResizeToContent()
        {
            float windowHeight = CalculateWindowHeight(targetSettings);

            minSize = new Vector2(WindowWidth, windowHeight);
            maxSize = new Vector2(WindowWidth, windowHeight);

            Rect currentPosition = position;
            currentPosition.width = WindowWidth;
            currentPosition.height = windowHeight;
            position = currentPosition;
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null || targetSettings == null)
                return;

            MonoScript script = GetFirstNonUnityScript(Selection.activeGameObject);

            SetTarget(targetSettings, Selection.activeGameObject, script);
            Repaint();
        }

        private void OnGUI()
        {
            if (targetSettings == null || targetObject == null || targetEntry == null)
            {
                Close();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.Space(6f);

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Object", targetObject, typeof(GameObject), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Current Row", EditorStyles.boldLabel);

            DrawCurrentRowCompact();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            DrawActionButtons();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            DrawPresetEditor();

            EditorGUILayout.EndScrollView();
        }

        private void DrawCurrentRowCompact()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            DrawIconField(ColumnAWidth);

            GUILayout.Space(ColumnGap);

            DrawBuiltinIconPopup(
                GUIContent.none,
                targetEntry.builtinIcon,
                selectedType =>
                {
                    Undo.RecordObject(targetSettings, "Set Pretty Hierarchy Built-in Icon");

                    targetEntry.builtinIcon = selectedType;
                    targetEntry.icon = null;

                    DetachTargetObjectFromPreset();
                    MarkDirty();
                },
                GUILayout.Width(ColumnBWidth));

            GUILayout.Space(ColumnGap);

            EditorGUI.BeginChangeCheck();

            Color colour = EditorGUILayout.ColorField(
                GUIContent.none,
                targetEntry.separatorColor,
                false,
                true,
                false,
                GUILayout.Width(ColumnCWidth));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSettings, "Change Pretty Hierarchy Colour");

                targetEntry.separatorColor = colour;

                DetachTargetObjectFromPreset();
                MarkDirty();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            bool showColourBar = GUILayout.Toggle(
                targetEntry.showColourBar,
                "Bar",
                GUILayout.Width(ColumnAWidth));

            GUILayout.Space(ColumnGap);

            bool showColourIcon = GUILayout.Toggle(
                targetEntry.showColourIcon,
                "Colour Icon",
                GUILayout.Width(ColumnBWidth));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSettings, "Change Pretty Hierarchy Colour Toggles");

                targetEntry.showColourBar = showColourBar;
                targetEntry.showColourIcon = showColourIcon;

                DetachTargetObjectFromPreset();
                MarkDirty();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawIconField(float width)
        {
            EditorGUI.BeginChangeCheck();

            Texture2D newIcon = (Texture2D)EditorGUILayout.ObjectField(
                targetEntry.icon,
                typeof(Texture2D),
                false,
                GUILayout.Width(width));

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(targetSettings, "Set Pretty Hierarchy Custom Icon");

            targetEntry.icon = newIcon;

            if (newIcon != null)
                targetEntry.builtinIcon = HierarchyIconSettings.BuiltinIconType.None;

            DetachTargetObjectFromPreset();
            MarkDirty();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+ Preset", GUILayout.Width(ColumnAWidth), GUILayout.Height(22f)))
                AddPreset();

            GUILayout.Space(ColumnGap);

            if (GUILayout.Button("Clear Icon", GUILayout.Width(ColumnBWidth), GUILayout.Height(22f)))
            {
                Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Object Icon");

                targetSettings.ClearObjectIcon(targetObject);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                DetachTargetObjectFromPreset();
                MarkDirty();
            }

            GUILayout.Space(ColumnGap);

            if (GUILayout.Button("Clear Colour", GUILayout.Width(ColumnCWidth), GUILayout.Height(22f)))
            {
                Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Object Colour");

                targetSettings.ClearObjectColour(targetObject);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                DetachTargetObjectFromPreset();
                MarkDirty();
            }

            GUILayout.Space(ColumnGap);

            if (GUILayout.Button("Reset", GUILayout.Width(ColumnDWidth), GUILayout.Height(22f)))
            {
                Undo.RecordObject(targetSettings, "Clear Pretty Hierarchy Entry");

                targetSettings.RemoveObjectEntry(targetObject);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                MarkDirty();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void AddPreset()
        {
            SerializedObject serializedSettings = new(targetSettings);
            SerializedProperty presetsProperty = serializedSettings.FindProperty("colourPresets");

            if (presetsProperty == null)
                return;

            serializedSettings.Update();

            Undo.RecordObject(targetSettings, "Add Pretty Hierarchy Preset");

            presetsProperty.arraySize++;

            SerializedProperty created = presetsProperty.GetArrayElementAtIndex(presetsProperty.arraySize - 1);

            created.FindPropertyRelative("displayName").stringValue = "New Preset";
            created.FindPropertyRelative("colour").colorValue = targetEntry.separatorColor;
            created.FindPropertyRelative("icon").objectReferenceValue = targetEntry.icon;
            created.FindPropertyRelative("builtinIcon").enumValueIndex = (int)targetEntry.builtinIcon;

            SerializedProperty showColourBarProperty = created.FindPropertyRelative("showColourBar");
            SerializedProperty showColourIconProperty = created.FindPropertyRelative("showColourIcon");

            if (showColourBarProperty != null)
                showColourBarProperty.boolValue = targetEntry.showColourBar;

            if (showColourIconProperty != null)
                showColourIconProperty.boolValue = targetEntry.showColourIcon;

            serializedSettings.ApplyModifiedProperties();

            MarkDirty();
            ResizeToContent();
        }

        private void DrawPresetEditor()
        {
            SerializedObject serializedSettings = new(targetSettings);
            SerializedProperty presetsProperty = serializedSettings.FindProperty("colourPresets");

            serializedSettings.Update();

            if (presetsProperty == null)
            {
                EditorGUILayout.HelpBox("Could not find colourPresets on HierarchyIconSettings.", MessageType.Error);
                return;
            }

            for (int i = 0; i < presetsProperty.arraySize; i++)
            {
                SerializedProperty presetProperty = presetsProperty.GetArrayElementAtIndex(i);
                DrawPresetEditorRow(serializedSettings, presetProperty, i);
                EditorGUILayout.Space(2f);
            }

            serializedSettings.ApplyModifiedProperties();
        }

        private void DrawPresetEditorRow(
            SerializedObject serializedSettings,
            SerializedProperty presetProperty,
            int index)
        {
            SerializedProperty displayNameProperty = presetProperty.FindPropertyRelative("displayName");
            SerializedProperty colourProperty = presetProperty.FindPropertyRelative("colour");
            SerializedProperty iconProperty = presetProperty.FindPropertyRelative("icon");
            SerializedProperty builtinIconProperty = presetProperty.FindPropertyRelative("builtinIcon");
            SerializedProperty showColourBarProperty = presetProperty.FindPropertyRelative("showColourBar");
            SerializedProperty showColourIconProperty = presetProperty.FindPropertyRelative("showColourIcon");

            HierarchyIconSettings.ColourPreset runtimePreset = index >= 0 && index < targetSettings.ColourPresets.Count
                ? targetSettings.ColourPresets[index]
                : null;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(PresetRowHeight));

            if (DrawAssignPresetButton(runtimePreset))
            {
                Undo.RecordObject(targetSettings, "Apply Pretty Hierarchy Preset");

                serializedSettings.ApplyModifiedProperties();

                targetSettings.ApplyPreset(targetObject, runtimePreset);
                targetEntry = targetSettings.GetOrCreateObjectEntry(targetObject);

                MarkDirty();
            }

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(displayNameProperty, GUIContent.none, GUILayout.MinWidth(150f));
            EditorGUILayout.PropertyField(iconProperty, GUIContent.none, GUILayout.MinWidth(170f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            HierarchyIconSettings.BuiltinIconType currentBuiltin =
                (HierarchyIconSettings.BuiltinIconType)builtinIconProperty.enumValueIndex;

            DrawBuiltinIconPopup(
                GUIContent.none,
                currentBuiltin,
                selectedType =>
                {
                    Undo.RecordObject(targetSettings, "Edit Pretty Hierarchy Preset Built-in Icon");

                    builtinIconProperty.enumValueIndex = (int)selectedType;

                    if (selectedType != HierarchyIconSettings.BuiltinIconType.None)
                        iconProperty.objectReferenceValue = null;

                    serializedSettings.ApplyModifiedProperties();

                    if (runtimePreset != null)
                        targetSettings.RefreshObjectsUsingPreset(runtimePreset);

                    MarkDirty();
                },
                GUILayout.MinWidth(170f));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Colour:", GUILayout.Width(48f));
            EditorGUILayout.PropertyField(colourProperty, GUIContent.none, GUILayout.Width(100f));

            GUILayout.Space(12f);

            if (showColourBarProperty != null)
                showColourBarProperty.boolValue = EditorGUILayout.ToggleLeft("Bar", showColourBarProperty.boolValue, GUILayout.Width(55f));

            if (showColourIconProperty != null)
                showColourIconProperty.boolValue = EditorGUILayout.ToggleLeft("Icon", showColourIconProperty.boolValue, GUILayout.Width(55f));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSettings, "Edit Pretty Hierarchy Preset");

                if (iconProperty.objectReferenceValue != null)
                    builtinIconProperty.enumValueIndex = (int)HierarchyIconSettings.BuiltinIconType.None;

                serializedSettings.ApplyModifiedProperties();

                if (runtimePreset != null)
                    targetSettings.RefreshObjectsUsingPreset(runtimePreset);

                MarkDirty();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("×", GUILayout.Width(DeleteButtonWidth), GUILayout.Height(PresetRowHeight)))
            {
                Undo.RecordObject(targetSettings, "Remove Pretty Hierarchy Preset");

                presetProperty.DeleteCommand();
                serializedSettings.ApplyModifiedProperties();

                MarkDirty();
                ResizeToContent();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private bool DrawAssignPresetButton(HierarchyIconSettings.ColourPreset preset)
        {
            Rect rect = GUILayoutUtility.GetRect(
                PreviewWidth,
                PresetRowHeight,
                GUILayout.Width(PreviewWidth),
                GUILayout.Height(PresetRowHeight));

            if (preset == null)
                return GUI.Button(rect, GUIContent.none);

            if (GUI.Button(rect, GUIContent.none))
                return true;

            Texture2D icon = preset.GetFinalIcon();

            if (preset.showColourBar)
            {
                Rect colourRect = new(rect.x + 7f, rect.y + 8f, rect.width - 14f, 18f);
                EditorGUI.DrawRect(colourRect, Color.black);
                EditorGUI.DrawRect(
                    new Rect(colourRect.x + 1f, colourRect.y + 1f, colourRect.width - 2f, colourRect.height - 2f),
                    preset.colour);
            }

            Rect iconBackRect = new(rect.x + 25f, rect.y + 36f, 20f, 20f);

            if (preset.showColourIcon)
                EditorGUI.DrawRect(iconBackRect, preset.colour);

            if (icon != null)
            {
                Rect iconRect = new(rect.x + 27f, rect.y + 38f, 16f, 16f);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }

            return false;
        }

        private void DrawBuiltinIconPopup(
            GUIContent label,
            HierarchyIconSettings.BuiltinIconType currentType,
            System.Action<HierarchyIconSettings.BuiltinIconType> selectedCallback,
            params GUILayoutOption[] options)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);

            Rect popupRect = controlRect;

            if (label != GUIContent.none)
            {
                Rect labelRect = new(
                    controlRect.x,
                    controlRect.y,
                    EditorGUIUtility.labelWidth * 0.55f,
                    controlRect.height);

                popupRect = new(
                    labelRect.xMax,
                    controlRect.y,
                    controlRect.width - labelRect.width,
                    controlRect.height);

                EditorGUI.LabelField(labelRect, label);
            }

            Texture2D selectedIcon = HierarchyIconSettings.GetBuiltinIcon(currentType);

            GUIContent buttonContent = new(
                currentType.ToString(),
                selectedIcon);

            if (GUI.Button(popupRect, buttonContent, EditorStyles.popup))
            {
                BuiltinIconDropdown.Open(
                    popupRect,
                    currentType,
                    selectedCallback);
            }
        }

        private void DetachTargetObjectFromPreset()
        {
            if (targetEntry == null)
                return;

            FieldInfo presetIdField = targetEntry
                .GetType()
                .GetField(
                    "presetId",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            if (presetIdField == null || presetIdField.FieldType != typeof(string))
                return;

            presetIdField.SetValue(targetEntry, string.Empty);
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(targetSettings);
            EditorApplication.RepaintHierarchyWindow();
            Repaint();
        }
    }
}