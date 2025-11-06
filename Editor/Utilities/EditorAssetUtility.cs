using System.IO;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Utilities
{
    /// <summary>
    /// Provides helper methods for creating and loading ScriptableObject assets within the Unity Editor.
    /// </summary>
    public static class EditorAssetUtility
    {
        #region Executes
        /// <summary>
        /// Creates a new ScriptableObject asset of type <typeparamref name="T"/> at the given path,
        /// or loads the existing one if it already exists.
        /// </summary>
        /// <typeparam name="T">The ScriptableObject type to create or load.</typeparam>
        /// <param name="path">The folder path where the asset should be created or loaded from.</param>
        /// <param name="fileName">The name of the asset file (without extension).</param>
        /// <returns>Returns the created or loaded ScriptableObject instance.</returns>
        public static T CreateOrLoadAsset<T>(string path, string fileName) where T : ScriptableObject
        {
            string fullPath = Path.Combine(path, fileName + ".asset");
            
            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            
            if (asset)
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