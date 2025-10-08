using System.IO;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Utilities
{
    public static class EditorAssetUtility
    {
        #region Executes
        public static T CreateOrLoadAsset<T>(string path, string fileName) where T : ScriptableObject
        {
            string fullPath = Path.Combine(path, fileName + ".asset");
            
            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            
            if (asset != null)
                return asset;
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            asset = ScriptableObject.CreateInstance<T>();
            
            asset.name = fileName;
            
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();

            return asset;
        }
        #endregion
    }
}