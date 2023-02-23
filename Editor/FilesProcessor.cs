using System.IO;
using UnityEngine;
using UnityEditor;

public class FilesProcessor : AssetPostprocessor
{
    void OnPreprocessAsset()
    {
        if (assetImporter.importSettingsMissing)
        {
            Debug.Log($"Outside modification => {assetImporter.assetPath}");
            if (!File.GetAttributes(assetImporter.assetPath).HasFlag(FileAttributes.Directory))
            {
                NamespaceResolver.OnWillMoveAsset(assetImporter.assetPath, assetImporter.assetPath);
            }
            // ModelImporter modelImporter = assetImporter as ModelImporter;
            // if (modelImporter != null)
            // {
            //     if (!assetPath.Contains("@"))
            //         modelImporter.importAnimation = false;
            //     modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            // }
        }
    }
}
