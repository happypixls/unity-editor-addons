using UnityEditor;
using UnityEngine;

namespace HappyPixels.EditorAddons
{
    public class FileCreationExtension : MonoBehaviour
    {
        [MenuItem("Assets/Create/C# Interface", false, 75)]
        private static void CreateNewCSharpInterface()
        {
            Debug.Log("Attempting to create interface from template");
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
