using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ItemBuildStudioDemo
{
    public class ItemBuildConfigBox : VisualElement
    {
        public readonly string[] platforms = new string[] { "Android", "iOS", "Windows", "MacOS" };

        private ItemBuildFormat itemBuildFormat;
        private ListView buildListView;
        private List<AssetBuildFormat> copiedBuildList;

        public int SelectPlatformIndex { get; private set; }
        public List<AssetBuildFormat> BuildList => itemBuildFormat.items[SelectPlatformIndex].buildList;

        public void CreateGUI()
        {
            // Title
            Label title = new Label() { name = "itemTitle", text = "Build Configuration" };

            // Option box
            VisualElement box = new VisualElement() { name = "box" };
            box.AddToClassList("build-config");

            #region Platform tab

            Label[] platformLabels = new Label[platforms.Length];
            VisualElement tab = new VisualElement() { name = "platformTab" };

            for (int i = 0; i < platforms.Length; i++)
            {
                Label platformLabel = new Label() { text = platforms[i] };
                platformLabel.AddToClassList("tab-button");
                if (i == platforms.Length - 1)
                    platformLabel.AddToClassList("last");

                int a = i;
                platformLabel.RegisterCallback<ClickEvent>(evt =>
                {
                    if (platformLabel.ClassListContains("selected"))
                        return;

                    SelectPlatformIndex = a;
                    UpdateAssetBuildListView();
                    for (int j = 0; j < platforms.Length; j++)
                    {
                        if (platformLabel.text == platforms[j])
                            platformLabels[j].AddToClassList("selected");
                        else
                            platformLabels[j].RemoveFromClassList("selected");
                    }
                });

                platformLabels[i] = platformLabel;
                tab.Add(platformLabel);
            }

            platformLabels[0].AddToClassList("selected");

            #endregion

            VisualElement buildList = new VisualElement() { name = "buildList" };

            // Control and Drag & Drop parent
            VisualElement controlBox = new VisualElement();
            controlBox.style.flexDirection = FlexDirection.Row;
            controlBox.style.justifyContent = Justify.SpaceBetween;

            // Button parent
            VisualElement control = new VisualElement();
            control.style.paddingTop = new StyleLength(10);
            control.style.flexDirection = FlexDirection.Row;
            control.style.alignItems = Align.Center;
            Label controlLabel = new Label("Build list");

            // Clear button
            Button clearButton = new Button(() =>
            {
                UpdateAssetBuildListView();
            })
            { text = "Clear" };

            // Paste button
            Button pasteButton = new Button(() =>
            {
                if (copiedBuildList != null)
                {
                    UpdateAssetBuildListView();
                }
            })
            { text = "Paste" };
            pasteButton.SetEnabled(false);

            // Copy button
            Button copyButton = new Button(() =>
            {

                pasteButton.SetEnabled(true);
            })
            { text = "Copy" };

            Button loadButton = new Button() { text = "Load" };

            control.Add(controlLabel);
            control.Add(clearButton);
            control.Add(copyButton);
            control.Add(pasteButton);
            control.Add(loadButton);
            controlBox.Add(control);

            // Drag & Drop
            Label dragDropBox = new Label("Drag & Drop") { name = "dragAndDropBox" };
            dragDropBox.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            dragDropBox.RegisterCallback<DragPerformEvent>(OnDragPerform);
            controlBox.Add(dragDropBox);

            buildListView = new ListView() { name = "assetListView" };
            buildListView.selectionType = SelectionType.Single;
            buildListView.fixedItemHeight = 100;
            buildListView.makeItem = MakeItem;
            buildListView.bindItem = BindItem;
#if UNITY_2022_1_OR_NEWER
            buildListView.selectionChanged += SelectionChanged;
#else
            buildListView.onSelectionChange += SelectionChanged;
#endif

            buildList.Add(controlBox);
            buildList.Add(buildListView);

            box.Add(tab);
            box.Add(buildList);

            Add(title);
            Add(box);
        }

        public void SetItemBuildConfig(ItemBuildFormat itemBuildFormat)
        {
            this.itemBuildFormat = itemBuildFormat;
            SelectPlatformIndex = 0;

            if (itemBuildFormat.items == null)
                itemBuildFormat.items = new Item[platforms.Length];

            UpdateAssetBuildListView();
            copiedBuildList?.Clear();
            copiedBuildList = null;
        }

        #region Drag & Drop events

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            foreach (string path in DragAndDrop.paths)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                var type = default(AssetType);
                var asset = new AssetBuildFormat(type, path, guid);

                // Check duplication
                if (!BuildList.Exists(x => x.fileName == asset.fileName && x.filePath == asset.filePath))
                {
                    asset.isMain = BuildList.Count == 0;
                    BuildList.Add(asset);
                    buildListView.Rebuild();
                }
            }
        }

        #endregion

        #region Build list func

        private VisualElement MakeItem()
        {
            VisualElement vi = new VisualElement() { name = "assetItem" };
            VisualElement left = new VisualElement() { name = "left" };
            VisualElement right = new VisualElement() { name = "right" };

            left.style.paddingRight = 120;
            left.style.flexGrow = 1;

            // Draw left column
            VisualElement fileNameIcon = new VisualElement() { name = "fileNameIcon" };
            fileNameIcon.style.flexDirection = FlexDirection.Row;

            // Icon
            VisualElement icon = new VisualElement() { name = "icon" };
            icon.style.width = 16;
            icon.style.height = 16;

            // File name
            Label name = new Label() { name = "name" };

            fileNameIcon.Add(icon);
            fileNameIcon.Add(name);
            left.Add(fileNameIcon);

            // Asset type
            var typeField = new EnumField("Type", default(AssetType)) { name = "type" };
            Toggle isUpdateToggle = new Toggle("No Version Update") { name = "version-update" };
            Toggle isMainToggle = new Toggle("Is Main") { name = "main-asset" };

            left.Add(typeField);
            left.Add(isUpdateToggle);
            left.Add(isMainToggle);

            // Remove
            Button removeButton = new Button() { name = "remove" };
            removeButton.style.width = 16;
            removeButton.style.height = 16;
            removeButton.style.backgroundImage = Background.FromTexture2D(EditorGUIUtility.FindTexture("d_winbtn_win_close@2x"));
            right.Add(removeButton);

            vi.Add(left);
            vi.Add(right);

            return vi;
        }

        private void BindItem(VisualElement vi, int index)
        {
            var format = itemBuildFormat.items[SelectPlatformIndex].buildList[index];

            // Set item information field
            var iconField = vi.Q("icon");
            iconField.style.backgroundImage = Background.FromTexture2D(AssetDatabase.GetCachedIcon(format.filePath) as Texture2D);
            vi.Q<Label>("name").text = Path.GetFileNameWithoutExtension(format.fileName);

            // Set version update field
            var updateField = vi.Q<Toggle>("version-update");
            updateField.value = format.noVersionUpdate;
            updateField.RegisterValueChangedCallback(evt => format.noVersionUpdate = evt.newValue);

            // Set main asset field
            var mainAssetField = vi.Q<Toggle>("main-asset");
            mainAssetField.value = format.isMain;
            mainAssetField.SetEnabled(!mainAssetField.value);
            mainAssetField.RegisterValueChangedCallback(evt =>
            {
                format.isMain = evt.newValue;
                if (evt.newValue)
                {
                    BuildList.Remove(format);
                    foreach (var other in BuildList)
                    {
                        other.isMain = false;
                    }
                    BuildList.Insert(0, format);
                    UpdateAssetBuildListView();
                }
            });

            // Set item type field
            var typeField = vi.Q<EnumField>("type");
            typeField.RegisterValueChangedCallback(evt => format.type = (AssetType)evt.newValue);

            // Set remove button action
            var removeButton = vi.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                if (BuildList.IndexOf(format) == 0 && BuildList.Count > 1)
                    BuildList[1].isMain = true;

                BuildList.Remove(format);
                UpdateAssetBuildListView();
            };
        }

        private void SelectionChanged(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var format = item as AssetBuildFormat;
                if (format != null)
                {
                    var target = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(format.filePath);
                    Selection.activeObject = target;
                }

                break;
            }
        }

        private void UpdateAssetBuildListView()
        {
            if (itemBuildFormat.items[SelectPlatformIndex] == null)
                itemBuildFormat.items[SelectPlatformIndex] = new Item();

            buildListView.itemsSource = BuildList;
            buildListView.Rebuild();
        }

        #endregion
    }
}