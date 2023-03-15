#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace HappyPixels.EditorAddons
{
    internal static class NamespaceResolver
    {
        internal static string GenerateNamespace(string metaFilePath)
        {
            var segmentedPath = Path.GetDirectoryName(metaFilePath)?.Split(Path.DirectorySeparatorChar);
            var initialNamespace = string.Join(".", segmentedPath?.Skip(2) ?? Array.Empty<string>());
            
            //Remove spaces in case folder names has spaces
            var generatedNamespaceWithoutSpaces = Regex.Replace(initialNamespace, @"\s+", "");
            return string.IsNullOrEmpty(generatedNamespaceWithoutSpaces)
                ? EditorSettings.projectGenerationRootNamespace
                : $"{EditorSettings.projectGenerationRootNamespace}.{generatedNamespaceWithoutSpaces}";
        }
    }
}
#endif