using UnityEditor;
using UnityEngine;

namespace HappyPixels.EditorAddons
{
    public class FileCreationExtension : MonoBehaviour
    {
        [MenuItem("Assets/Create/C# Class", false, 74)]
        private static void CreateNewCSharpClass()
        {
            NamespaceResolver.CurrentlyCreatedFile = FileType.CSharpClass;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpClassTemplate.cs.txt",
                "Class.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Interface", false, 75)]
        private static void CreateNewCSharpInterface()
        {
            NamespaceResolver.CurrentlyCreatedFile = FileType.InterfaceFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpInterfaceTemplate.cs.txt", 
                "IInterface.cs");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/Create/C# Enum", false, 76)]
        private static void CreateNewCSharpEnum()
        {
            NamespaceResolver.CurrentlyCreatedFile = FileType.EnumFile;
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpEnumTemplate.cs.txt",
                "Enum.cs");
            AssetDatabase.Refresh();
        }
    }
}
