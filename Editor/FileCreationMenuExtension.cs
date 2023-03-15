using UnityEditor;
using UnityEngine;

namespace HappyPixels.EditorAddons
{
    public class FileCreationMenuExtension : MonoBehaviour
    {
        [MenuItem("Assets/Create/C# Class", false, 74)]
        private static void CreateNewCSharpClass()
        {
            TemplateGenerationManagement.CurrentlyCreatedFile = FileType.CSharpClass;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Constants.DEFAULT_CSHARP_CLASS_TEMPLATE_PATH, "Class.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Interface", false, 75)]
        private static void CreateNewCSharpInterface()
        {
            TemplateGenerationManagement.CurrentlyCreatedFile = FileType.InterfaceFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Constants.DEFAULT_CSHARP_INTERFACE_TEMPLATE_PATH, "IInterface.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Enum", false, 76)]
        private static void CreateNewCSharpEnum()
        {
            TemplateGenerationManagement.CurrentlyCreatedFile = FileType.EnumFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Constants.DEFAULT_CSHARP_ENUM_TEMPLATE_PATH, "Enum.cs");
            AssetDatabase.Refresh();
        }
    }
}
