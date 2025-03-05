using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ItemBuildStudioDemo
{
    public enum ItemCategory
    {
        Temp_1,
        Temp_2,
        Temp_3
    }

    public enum AssetType
    {
        Metadata,
        Prefab,
        Audio,
        Text
    }

    [Serializable]
    public class ItemBuildFormat
    {
        public string name;
        public string pid;
        public string title;
        public ItemCategory category;
        public string description;
        public string iconPath;
        public List<string> screenShotPaths = new List<string>();
        public BuildTarget buildTarget;

        public Item[] items;
    }

    [Serializable]
    public class Item
    {
        public List<AssetBuildFormat> buildList = new List<AssetBuildFormat>();
    }

    [Serializable]
    public class AssetBuildFormat
    {
        public string filePath;
        public string fileName;
        public string guid;
        public bool isMain;
        public bool noVersionUpdate;
        public AssetType type;

        public AssetBuildFormat(AssetType type, string filePath, string guid)
        {
            this.filePath = filePath;
            this.guid = guid;
            this.type = type;

            fileName = Path.GetFileName(filePath);
        }
    }
}
