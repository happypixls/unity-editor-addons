using System.IO;
using UnityEngine;
using UnityEditor;

namespace HappyPixels.EditorAddons
{
    public class FilesProcessor : AssetPostprocessor
    {
        private void OnPreprocessAsset()
        {
            if (assetImporter.importSettingsMissing)
            {
                Debug.Log($"Outside modification => {assetImporter.assetPath}");
                if (!File.GetAttributes(assetImporter.assetPath).HasFlag(FileAttributes.Directory))
                {
                    NamespaceResolver.OnWillMoveAsset(assetImporter.assetPath, assetImporter.assetPath);
                }
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            Debug.Log($"<color=purple>OnProcessallAssets is called</color> imported = {importedAssets.Length}," +
                        $" deleted = {deletedAssets.Length}, moved = {movedAssets.Length}, movedFromAssetPaths = {movedFromAssetPaths.Length}");
        }
    }
}
