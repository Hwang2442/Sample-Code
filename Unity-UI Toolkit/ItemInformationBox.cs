using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;

namespace ItemBuildStudioDemo
{
    public class ItemInformationBox : VisualElement
    {
        private ItemBuildFormat itemBuildFormat;

        private TextField itemNameField;
        private Button itemNameApplyButton;
        private TextField pidField;
        private TextField titleField;
        private EnumField categoryField;
        private TextField descField;
        private ObjectField iconField;
        private ObjectField[] screenshotFields;

        public void CreateGUI()
        {
            // Title
            Label title = new Label() { name = "itemTitle", text = "Item Information" };

            // Option box
            VisualElement box = new VisualElement() { name = "box" };

            // Name field & Apply button
            VisualElement itemNameEditField = new VisualElement();
            itemNameEditField.style.flexDirection = FlexDirection.Row;
            itemNameEditField.style.justifyContent = Justify.SpaceBetween;

            itemNameField = new TextField("Item Name");
            itemNameField.style.flexGrow = 1;

            itemNameApplyButton = new Button() { text = "Apply" };

            itemNameEditField.Add(itemNameField);
            itemNameEditField.Add(itemNameApplyButton);

            // PID
            pidField = new TextField("PID") { value = "PD_1231456489", isReadOnly = true };

            // Title
            titleField = new TextField("Title");
            titleField.RegisterValueChangedCallback(evt => { itemBuildFormat.title = evt.newValue; });

            // Category
            categoryField = new EnumField("Category", default(ItemCategory));
            categoryField.RegisterValueChangedCallback(evt => { itemBuildFormat.category = (ItemCategory)evt.newValue; });

            // Description
            descField = new TextField("Description") { value = "default description" };
            descField.RegisterValueChangedCallback(evt => { itemBuildFormat.description = evt.newValue; });

            // Icon
            iconField = new ObjectField("Icon") { objectType = typeof(Texture) };
            iconField.RegisterValueChangedCallback(evt => { 
                itemBuildFormat.iconPath = evt.newValue ? AssetDatabase.GetAssetPath(evt.newValue) : ""; 
            });

            // Screenshots
            VisualElement screenshotBox = new VisualElement() { name = "screenshot" };
            screenshotBox.style.flexDirection = FlexDirection.Row;
            screenshotBox.style.flexGrow = 1;
            screenshotFields = new ObjectField[5];
            for (int i = 0; i < 5; i++)
            {
                screenshotFields[i] = new ObjectField(i == 0 ? "Screenshots" : "") { objectType = typeof(Texture) };
                screenshotFields[i].style.flexGrow = 1;
                screenshotFields[i].style.flexShrink = 1;
                screenshotFields[i].RegisterValueChangedCallback(evt => {

                });
                screenshotBox.Add(screenshotFields[i]);
            }

            box.Add(itemNameEditField);
            box.Add(pidField);
            box.Add(titleField);
            box.Add(categoryField);
            box.Add(descField);
            box.Add(iconField);
            box.Add(screenshotBox);

            Add(title);
            Add(box);
        }

        public void SetItemInformation(ItemBuildFormat itemBuildFormat, Action onItemNameEdited)
        {
            this.itemBuildFormat = itemBuildFormat;

            itemNameField.value = itemBuildFormat.name;
            pidField.value = itemBuildFormat.pid;
            titleField.value = itemBuildFormat.title;
            categoryField.value = itemBuildFormat.category;
            iconField.value = AssetDatabase.LoadAssetAtPath(itemBuildFormat.iconPath, typeof(Texture));

            if (itemBuildFormat.screenShotPaths.Count <= 0)
                itemBuildFormat.screenShotPaths = new List<string>(Enumerable.Repeat("", 5));

            for (int i = 0; i < itemBuildFormat.screenShotPaths.Count; i++)
            {
                string path = itemBuildFormat.screenShotPaths[i];
                if (!string.IsNullOrEmpty(path))
                {
                    screenshotFields[i].value = AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
                }
            }
        }
    }
}