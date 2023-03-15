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
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpClassTemplate.cs.txt",
                "Class.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Interface", false, 75)]
        private static void CreateNewCSharpInterface()
        {
            TemplateGenerationManagement.CurrentlyCreatedFile = FileType.InterfaceFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpInterfaceTemplate.cs.txt", 
                "IInterface.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Enum", false, 76)]
        private static void CreateNewCSharpEnum()
        {
            TemplateGenerationManagement.CurrentlyCreatedFile = FileType.EnumFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpEnumTemplate.cs.txt",
                "Enum.cs");
            AssetDatabase.Refresh();
        }
    }
}
