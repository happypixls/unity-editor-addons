#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace HappyPixels.EditorAddons
{
    public enum FileType
    {
        None = -1,
        CSharpScript = 0,
        InterfaceFile = 1,
        EnumFile = 2,
        Asmdef = 3,
    }
    
    public class NamespaceResolver : AssetModificationProcessor
    {
        public static FileType CurrentlyCreatedFile { get; set; } = FileType.None;
        private static Dictionary<FileType, Action<string, string>> FileTemplatesGenerators { get; } = 
            new() 
            {
                { FileType.CSharpScript, GenerateCSharpMonobehaviourScript },
                { FileType.InterfaceFile, GenerateCSharpInterface },
                { FileType.EnumFile, GenerateCSharpEnum },
                { FileType.Asmdef, GenerateAssemblyDefinition },
            };
        
        private static string GenerateNamespace(string metaFilePath)
        {
            var segmentedPath = $"{Path.GetDirectoryName(metaFilePath)}".Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.None);

            var generatedNamespace = "";
            var finalNamespace = "";

            // In case of placing the class at the root of a folder such as (Editor, Scripts, etc...)  
            if (segmentedPath.Length <= 2)
                finalNamespace = EditorSettings.projectGenerationRootNamespace;
            else
            {
                // Skipping the Assets folder and a single subfolder (i.e. Scripts, Editor, Plugins, etc...)
                for (var i = 2; i < segmentedPath.Length; i++)
                {
                    generatedNamespace +=
                        i == segmentedPath.Length - 1
                            ? segmentedPath[i]        // Don't add '.' at the end of the namespace
                            : segmentedPath[i] + "."; 
                }
                
                finalNamespace = EditorSettings.projectGenerationRootNamespace + "." + generatedNamespace;
            }
            
            return finalNamespace;
        }

        private static void OnWillCreateAsset(string metaFilePath)
        {
            if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace))
            {
                Debug.LogError("Root Namespace is not set in project settings, Namespace resolver is aborted.");
                return;
            }
            
            var fileName = Path.GetFileNameWithoutExtension(metaFilePath);

            if (metaFilePath.EndsWith(".cs.meta") )
            {
                switch (CurrentlyCreatedFile)
                {
                    case FileType.None:
                        FileTemplatesGenerators[FileType.CSharpScript].Invoke(metaFilePath, fileName);
                        break;
                    default:
                        FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                        break;
                }

                CurrentlyCreatedFile = FileType.None;
                return;
            }
            
            if (metaFilePath.EndsWith(".asmdef.meta"))
            {
                Debug.Log("Attempting to create assembly definition file");
                CurrentlyCreatedFile = FileType.Asmdef;
                FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                
                CurrentlyCreatedFile = FileType.None; //This is set in order not to conflict with cs interface creation or enums 
                return;
            }
            
        }

        private static void GenerateCSharpMonobehaviourScript(string metaFilePath, string fileName)
        {
            Debug.Log("===================> Generating a namespace");
            var finalNamespace = GenerateNamespace(metaFilePath);
            var actualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{fileName}";
            var myTemplate =
                File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpMonobehaviourTemplate.cs.txt");
            var newContent = myTemplate.Replace("#NAMESPACE#", Regex.Replace(finalNamespace, @"\b \b", ""))
                .Replace("#SCRIPTNAME#", fileName.Substring(0, fileName.IndexOf('.')));

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }

        private static void GenerateCSharpInterface(string metaFilePath, string fileName)
        {
            var finalNamespace = GenerateNamespace(metaFilePath);
            var actualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{fileName}";
            var myTemplate =
                File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpInterfaceTemplate.cs.txt");
            var newContent = myTemplate.Replace("#NAMESPACE#", Regex.Replace(finalNamespace, @"\b \b", ""))
                .Replace("#SCRIPTNAME#", fileName.Substring(0, fileName.IndexOf('.')));

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
        
        private static void GenerateCSharpEnum(string metaFilePath, string fileName)
        {
            var finalNamespace = GenerateNamespace(metaFilePath);
            var actualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{fileName}";
            var myTemplate =
                File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpEnumTemplate.cs.txt");
            var newContent = myTemplate.Replace("#NAMESPACE#", Regex.Replace(finalNamespace, @"\b \b", ""))
                .Replace("#SCRIPTNAME#", fileName.Substring(0, fileName.IndexOf('.')));

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
        
        private static void GenerateAssemblyDefinition(string metaFilePath, string fileName)
        {
            Debug.Log("<color=yellow> Creating assembly definition file </color>");
            var finalNamespace = GenerateNamespace(metaFilePath);
            var actualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{fileName}";
            var myTemplate =
                File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/AssemblyDefinitionTemplate.asmdef.txt");
            var newContent = myTemplate.Replace("#NAMESPACE#", $"\"{Regex.Replace(finalNamespace, @"\b \b", "")}\"");
            newContent = newContent.Replace("#ROOTNAMESPACE#", $"\"{EditorSettings.projectGenerationRootNamespace}\"");

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
        
        public static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.Directory))
            {
                MoveFolderWithContent(sourcePath, destinationPath);
            }
            else if (sourcePath.EndsWith(".cs"))
            {
                ChangeOrAddLine(sourcePath, GenerateNamespace(destinationPath + ".meta"), "namespace");

                File.Move(sourcePath, destinationPath);
                File.Move(sourcePath + ".meta", destinationPath + ".meta");
            }
            else
            {
                File.Move(sourcePath, destinationPath);
                File.Move(sourcePath + ".meta", destinationPath + ".meta");
            }
            
            return AssetMoveResult.DidMove;
        }

        private static void MoveFolderWithContent(string sourcePath, string destinationPath)
        {
            Debug.Log("<color=yellow> Moving folder with content </color>");
            Directory.Move(sourcePath, destinationPath);
            Directory.EnumerateFiles(destinationPath).ToList().ForEach(file =>
            {
                Debug.Log($"<color=green>{file}</color>");
                if (file.EndsWith(".asmdef"))
                {
                    //TODO: Change this to ChangeOrAddLine custom method for asemdef files
                    FileTemplatesGenerators[FileType.Asmdef].Invoke(file + ".meta", Path.GetFileName(file));
                }
                else if (file.EndsWith(".cs"))
                {
                    ChangeOrAddLine(file, GenerateNamespace(destinationPath + Path.DirectorySeparatorChar), "namespace");
                }
            }); 
            File.Move(sourcePath + ".meta", destinationPath + ".meta");
        }

        private static void ChangeOrAddLine(string filePath, string newLine, string beginningTextLine = "")
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            using var sr = new StreamReader(fs);
            using var sw = new StreamWriter(fs);

            var lines = sr.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            fs.Position = 0;
            if (beginningTextLine != "")
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    var splitLine = lines[i].Split(' ');
                    if (splitLine[0] == beginningTextLine)
                    {
                        splitLine[1] = newLine;
                        lines[i] = splitLine[0] + " " + splitLine[1];
                        break;
                    }
                }
            }
            sw.Write(string.Join("\r\n", lines));
            fs.SetLength(fs.Position);
        }
    }
}
#endif