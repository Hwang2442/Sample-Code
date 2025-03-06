using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ItemBuildStudioDemo
{
    public class ItemBuildStudioDemo : EditorWindow
    {
        private List<ItemBuildFormat> itemList = new List<ItemBuildFormat>();

        private ListView itemListView;
        private string searchItemNameValue = "";
        private List<ItemBuildFormat> displayItemList = new List<ItemBuildFormat>();
        private ItemBuildFormat selectedItemFormat;

        private VisualElement itemDetailContainer;
        private ItemInformationBox itemInformationBox;
        private ItemBuildConfigBox itemBuildConfigBox;

        [MenuItem("Tools/UI Toolkit/ItemBuildStudio - Demo")]
        public static void ShowExample()
        {
            ItemBuildStudioDemo wnd = GetWindow<ItemBuildStudioDemo>();
            wnd.titleContent = new GUIContent("ItemBuildStudio - Demo");
        }

        private void Demo()
        {
            string path = "Assets/ItemBuildStudio Demo/Demo/01_Default_Item.json";
            var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            itemList.Add(JsonUtility.FromJson<ItemBuildFormat>(jsonAsset.text));
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Apply uss
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemBuildStudio Demo/Editor/ItemBuildStudioDemo.uss");
            root.styleSheets.Add(styleSheet);

            // Create studio UI
            CreateVisualSplitter();

            Demo();
            UpdateItemListView();

            Debug.Log("Create Editor UI");
        }

        private void CreateVisualSplitter()
        {
            // Main container
            VisualElement main = new TwoPaneSplitView(0, 400, TwoPaneSplitViewOrientation.Horizontal) { name = "mainContainer" };

            // Left column container (Item list view)
            VisualElement left = new VisualElement() { name = "leftColumnContainer" };
            left.style.minWidth = 400;
            left.style.maxWidth = 400;
            main.Add(left);

            VisualElement right = new VisualElement() { name = "rightColumnContainer" };
            right.style.minWidth = 500;
            main.Add(right);

            CreateLeftColumn(left);
            CreateRightColumn(right);
            rootVisualElement.Add(main);
        }

        /// <summary>
        /// Create Item list view
        /// </summary>
        /// <param name="vi"></param>
        private void CreateLeftColumn(VisualElement vi)
        {
            #region Toolbar

            // Load button texture
            var createButtonTexture = EditorGUIUtility.FindTexture("d_Toolbar Plus");
            var refreshButtonTexture = EditorGUIUtility.FindTexture("d_Refresh");
            var deleteButtonTexture = EditorGUIUtility.FindTexture("d_Toolbar Minus");

            Toolbar toolbar = new Toolbar() { name = "toolbar" };
            toolbar.style.minWidth = 350;
            toolbar.style.maxWidth = 500;

            // Create button
            var createButton = new ToolbarButton(CreateItemBuildFormat) { name = "add" };
            createButton.AddToClassList("toolbar-button");
            createButton.tooltip = "积己";
            createButton.style.backgroundImage = Background.FromTexture2D(createButtonTexture);
            toolbar.Add(createButton);

            // Refresh button
            var refreshButton = new ToolbarButton(UpdateItemListView) { name = "refresh" };
            refreshButton.AddToClassList("toolbar-button");
            refreshButton.tooltip = "货肺绊魔";
            refreshButton.style.backgroundImage = Background.FromTexture2D(refreshButtonTexture);
            toolbar.Add(refreshButton);

            // Delete button
            var deleteButton = new ToolbarButton(DeleteItemBuildFormat) { name = "delete" };
            deleteButton.AddToClassList("toolbar-button");
            deleteButton.tooltip = "昏力";
            deleteButton.SetEnabled(false);
            deleteButton.style.backgroundImage = Background.FromTexture2D(deleteButtonTexture);
            toolbar.Add(deleteButton);

            // Space
            var spacer = new ToolbarSpacer() { flex = true };
            spacer.style.width = 6f;
            spacer.style.height = 1f;
            toolbar.Add(spacer);

            // Search field
            var searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(OnItemSearchFieldValueChanged);
            toolbar.Add(searchField);

            vi.Add(toolbar);

            #endregion

            #region Item list view

            Func<VisualElement> makeItem = () => new Label() { name = "item" };
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                Label label = e as Label;
                label.text = displayItemList[i].name;
                label.userData = displayItemList[i];
            };

            itemListView = new ListView(displayItemList, 25, makeItem, bindItem);
            itemListView.style.flexGrow = 1;
            itemListView.fixedItemHeight = 25;
            itemListView.selectionType = SelectionType.Single;

#if UNITY_2022_1_OR_NEWER
            itemListView.selectionChanged += (objs) => { deleteButton.SetEnabled(true); OnItemSelectionChange(objs);  };
#else
            itemListView.onSelectionChange += (objs) => { deleteButton.SetEnabled(true); OnItemSelectionChange(objs); };
#endif

            vi.Add(itemListView);

            #endregion
        }

        private void CreateRightColumn(VisualElement vi)
        {
            #region Toolbar

            Toolbar toolbar = new Toolbar() { name = "toolBar" };
            toolbar.style.flexDirection = FlexDirection.RowReverse;

            // Toolbar buttons
            var buildAndUpload = new ToolbarButton() { name = "buildAndUpload", text = "Build and Upload" };
            var buildButton = new ToolbarButton() { name = "build", text = "Build" };
            var saveButton = new ToolbarButton(SaveItemBuildFormat) { name = "save", text = "Save" };
            var uploadButton = new ToolbarButton() { name = "upload", text = "Upload" };
            var explorerButton = new ToolbarButton() { name = "explorer", text = "Explorer" };

            toolbar.Add(buildAndUpload);
            toolbar.Add(buildButton);
            toolbar.Add(saveButton);
            toolbar.Add(uploadButton);
            toolbar.Add(explorerButton);

            vi.Add(toolbar);

            #endregion

            itemDetailContainer = new VisualElement() { name = "rightContainer", visible = false };
            itemDetailContainer.style.flexGrow = 1;

            itemInformationBox = new ItemInformationBox();
            itemInformationBox.CreateGUI();
            itemDetailContainer.Add(itemInformationBox);

            itemBuildConfigBox = new ItemBuildConfigBox();
            itemBuildConfigBox.style.flexGrow = 1;
            itemBuildConfigBox.CreateGUI();
            itemDetailContainer.Add(itemBuildConfigBox);

            vi.Add(itemDetailContainer);
        }

        #region Item list view Toolbar features

        private void CreateItemBuildFormat()
        {
            string format = "{0:D2}_Default_Item";
            for (int i = 1; i < 100; i++)
            {
                string name = string.Format(format, i);
                if (!itemList.Any(x => x.name == name))
                {
                    itemList.Add(new ItemBuildFormat() { name = name });
                    UpdateItemListView();

                    break;
                }
            }
        }

        private void UpdateItemListView()
        {
            // Item listView refresh
            displayItemList.Clear();
            foreach (var item in itemList)
            {
                if (searchItemNameValue == "" || item.name.Contains(searchItemNameValue))
                    displayItemList.Add(item);
            }

            itemDetailContainer.visible = selectedItemFormat != null;
            itemListView.RefreshItems();
        }

        private void DeleteItemBuildFormat()
        {
            if (selectedItemFormat != null)
            {
                itemList.Remove(selectedItemFormat);
                selectedItemFormat = null;

                UpdateItemListView();
            }
        }

        private void OnItemSearchFieldValueChanged(ChangeEvent<string> e)
        {
            searchItemNameValue = e.newValue;
            UpdateItemListView();
        }

        #endregion

        private void OnItemSelectionChange(IEnumerable<object> e)
        {
            foreach (var item in e)
            {
                // Get selected item
                selectedItemFormat = item as ItemBuildFormat;
                itemDetailContainer.visible = selectedItemFormat != null;
                itemInformationBox.SetItemInformation(selectedItemFormat, UpdateItemListView);
                itemBuildConfigBox.SetItemBuildConfig(selectedItemFormat);

                break;
            }
        }

        private void SaveItemBuildFormat()
        {
            if (selectedItemFormat != null)
            {
                string path = Path.Combine(Application.dataPath, "ItemBuildStudio Demo/Demo", selectedItemFormat.name + ".json");
                string jsonString = JsonUtility.ToJson(selectedItemFormat);
                File.WriteAllText(path, jsonString);

                AssetDatabase.Refresh();
            }
        }
    }
}
