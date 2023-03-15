using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace HappyPixels.EditorAddons
{
    internal static class TemplateGenerationManagement
    {
        internal static FileType CurrentlyCreatedFile { get; set; } = FileType.CSharpScript;
        
        internal static void GenerateCSharpMonobehaviourScript(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, Constants.DEFAULT_MONOBEHAVIOUR_TEMPLATE_PATH);

        internal static void GenerateCSharpClass(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, Constants.DEFAULT_CSHARP_CLASS_TEMPLATE_PATH);

        internal static void GenerateCSharpInterface(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, Constants.DEFAULT_CSHARP_INTERFACE_TEMPLATE_PATH);

        internal static void GenerateCSharpEnum(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, Constants.DEFAULT_CSHARP_ENUM_TEMPLATE_PATH);

        internal static void GenerateAssemblyDefinition(string metaFilePath, string fileName)
        {
            var actualFile = Path.Combine(Path.GetDirectoryName(metaFilePath), fileName);
            var myTemplate = File.ReadAllText(Constants.DEFAULT_ASMDEF_TEMPLATE_PATH);
            var finalNamespace = NamespaceResolver.GenerateNamespace(metaFilePath);
            var newContent = myTemplate
                .Replace("#NAMESPACE#", $"\"{Regex.Replace(finalNamespace, @"\b \b", "")}\"")
                .Replace("#ROOTNAMESPACE#", $"\"{EditorSettings.projectGenerationRootNamespace}\"");

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
        
        private static void GenerateScript(string metaFilePath, string fileName, string templatePath)
        {
            var finalNamespace = NamespaceResolver.GenerateNamespace(metaFilePath);
            var actualFile = Path.Combine(Path.GetDirectoryName(metaFilePath), fileName);
            var myTemplate = File.ReadAllText(templatePath);
            var newContent = myTemplate
                .Replace("#NAMESPACE#", Regex.Replace(finalNamespace, @"\b \b", ""))
                .Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(fileName));

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
    }
}