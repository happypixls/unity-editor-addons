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
        CSharpClass = 1,
        InterfaceFile = 2,
        EnumFile = 3,
        Asmdef = 4,
    }
    
    public class NamespaceResolver : AssetModificationProcessor
    {
        internal static FileType CurrentlyCreatedFile { get; set; } = FileType.None;
        private static Dictionary<FileType, Action<string, string>> FileTemplatesGenerators { get; } = 
            new() 
            {
                { FileType.CSharpScript, GenerateCSharpMonobehaviourScript },
                { FileType.CSharpClass, GenerateCSharpClass },
                { FileType.InterfaceFile, GenerateCSharpInterface },
                { FileType.EnumFile, GenerateCSharpEnum },
                { FileType.Asmdef, GenerateAssemblyDefinition },
            };
        
        private static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        private static bool IsCsFile(string path) => path.EndsWith(".cs");
        private static bool IsAsmdefFile(string path) => path.EndsWith(".asmdef");
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
            
            if (fileName.EndsWith(".cs") )
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
            
            if (fileName.EndsWith(".asmdef"))
            {
                CurrentlyCreatedFile = FileType.Asmdef;
                FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                
                CurrentlyCreatedFile = FileType.None; //This is set in order not to conflict with cs interface creation or enums 
            }
        }

        private static void GenerateScript(string metaFilePath, string fileName, string templatePath)
        {
            var finalNamespace = GenerateNamespace(metaFilePath);
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

        private static void GenerateCSharpMonobehaviourScript(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, "Packages/com.happypixels.editoraddons/Editor/Templates/CSharpMonobehaviourTemplate.cs.txt");

        private static void GenerateCSharpClass(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, "Packages/com.happypixels.editoraddons/Editor/Templates/CSharpClassTemplate.cs.txt");
        
        private static void GenerateCSharpInterface(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, "Packages/com.happypixels.editoraddons/Editor/Templates/CSharpInterfaceTemplate.cs.txt");

        private static void GenerateCSharpEnum(string metaFilePath, string fileName) =>
            GenerateScript(metaFilePath, fileName, "Packages/com.happypixels.editoraddons/Editor/Templates/CSharpEnumTemplate.cs.txt");
        
        private static void GenerateAssemblyDefinition(string metaFilePath, string fileName)
        {
            var actualFile = Path.Combine(Path.GetDirectoryName(metaFilePath), fileName);
            var myTemplate = File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/AssemblyDefinitionTemplate.asmdef.txt");
            var finalNamespace = GenerateNamespace(metaFilePath);
            var newContent = myTemplate
                .Replace("#NAMESPACE#", $"\"{Regex.Replace(finalNamespace, @"\b \b", "")}\"")
                .Replace("#ROOTNAMESPACE#", $"\"{EditorSettings.projectGenerationRootNamespace}\"");

            if (myTemplate != newContent)
            {
                File.WriteAllText(actualFile, newContent);
                AssetDatabase.Refresh();
            }
        }
        
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (IsDirectory(sourcePath))
            {
                MoveFolderWithContent(sourcePath, destinationPath);
                MoveMetaFile(sourcePath, destinationPath);
                return AssetMoveResult.DidMove;
            }

            if (IsCsFile(sourcePath)) 
                ChangeNamespace(sourcePath, destinationPath);

            if (IsAsmdefFile(sourcePath))
                ChangeAsmdefContent(sourcePath, destinationPath);

            MoveFile(sourcePath, destinationPath);
            MoveMetaFile(sourcePath, destinationPath);

            return AssetMoveResult.DidMove;
        }
        
        private static void ChangeNamespace(string filePath, string destinationPath)
        {
            var generatedNamespace = GenerateNamespace(destinationPath + ".meta");
            ChangeOrAddLine(filePath, generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
        }

        private static void ChangeAsmdefContent(string filePath, string destinationPath)
        {
            var generatedName = GenerateNamespace(destinationPath + ".meta");
            ChangeOrAddLine(filePath, $"\"{generatedName}\",", "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
        }

        private static void MoveFile(string sourcePath, string destinationPath) => 
            File.Move(sourcePath, destinationPath);
        private static void MoveMetaFile(string sourcePath, string destinationPath) => 
            File.Move(sourcePath + ".meta", destinationPath + ".meta");
        
        private static void MoveFolderWithContent(string sourcePath, string destinationPath)
        {
            Directory.Move(sourcePath, destinationPath);
            
            Directory.EnumerateFiles(destinationPath, "*.cs", SearchOption.AllDirectories)
                .Union(Directory.EnumerateFiles(destinationPath, "*.asmdef", SearchOption.AllDirectories))
                .ToList()
                .ForEach(file =>
                {
                    if (file.EndsWith(".asmdef"))
                    {
                        var generatedNamespace = GenerateNamespace(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar);
                        ChangeOrAddLine(file, generatedNamespace, "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
                    }
                    else if (file.EndsWith(".cs"))
                    {
                        var generatedNamespace = GenerateNamespace(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar);
                        ChangeOrAddLine(file,generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
                    }
                });
        }
        
        private static void ChangeOrAddLine(string filePath, string newLine, string beginningTextLine, char splittingChar, Func<string, string, char, string> returnedFormattedLine)
        {
            var lines = File.ReadAllLines(filePath).ToList();

            if (!string.IsNullOrEmpty(beginningTextLine))
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    var splitLine = lines[i].Split(splittingChar);
                    var lineWithoutSpaces = Regex.Replace(splitLine[0], @"\s+", "");

                    if (lineWithoutSpaces == beginningTextLine)
                    {
                        splitLine[1] = newLine;
                        lines[i] = returnedFormattedLine(splitLine[0], splitLine[1], splittingChar);
                        break;
                    }
                }
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
#endif