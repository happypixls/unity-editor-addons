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
        CSharpScript = 0,
        CSharpClass = 1,
        InterfaceFile = 2,
        EnumFile = 3,
        Asmdef = 4,
    }
    
    public class NamespaceResolver : AssetModificationProcessor
    {
        internal static FileType CurrentlyCreatedFile { get; set; } = FileType.CSharpScript;
        private static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        private static bool IsCsFile(string path) => path.EndsWith(".cs");
        private static bool IsAsmdefFile(string path) => path.EndsWith(".asmdef");
        
        private static Dictionary<FileType, Action<string, string>> FileTemplatesGenerators { get; } = 
            new() 
            {
                { FileType.CSharpScript, GenerateCSharpMonobehaviourScript },
                { FileType.CSharpClass, GenerateCSharpClass },
                { FileType.InterfaceFile, GenerateCSharpInterface },
                { FileType.EnumFile, GenerateCSharpEnum },
                { FileType.Asmdef, GenerateAssemblyDefinition },
            };

        private static string GenerateNamespace(string metaFilePath)
        {
            var segmentedPath = Path.GetDirectoryName(metaFilePath)?.Split(Path.DirectorySeparatorChar);
            var generatedNamespace = string.Join(".", segmentedPath?.Skip(2) ?? Array.Empty<string>());
            return string.IsNullOrEmpty(generatedNamespace)
                ? EditorSettings.projectGenerationRootNamespace
                : $"{EditorSettings.projectGenerationRootNamespace}.{generatedNamespace}";
        }

        private static void OnWillCreateAsset(string metaFilePath)
        {
            if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace))
            {
                Debug.LogError("Root Namespace is not set in project settings, Namespace resolver is aborted.");
                return;
            }
            
            var fileName = Path.GetFileNameWithoutExtension(metaFilePath);
            
            if (IsCsFile(fileName))
            {
                switch (CurrentlyCreatedFile)
                {
                    case FileType.CSharpScript:
                        FileTemplatesGenerators[FileType.CSharpScript].Invoke(metaFilePath, fileName);
                        break;
                    default:
                        FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                        break;
                }

                CurrentlyCreatedFile = FileType.CSharpScript;
                return;
            }
            
            if (IsAsmdefFile(fileName))
            {
                CurrentlyCreatedFile = FileType.Asmdef;
                FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                
                CurrentlyCreatedFile = FileType.CSharpScript; //This is set in order not to conflict with cs interface, enums or POC creation
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
            var generatedNamespace = GenerateNamespace($"{destinationPath}.meta");
            ChangeOrAddLine(filePath, generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
        }

        private static void ChangeAsmdefContent(string filePath, string destinationPath)
        {
            var generatedName = GenerateNamespace($"{destinationPath}.meta");
            ChangeOrAddLine(filePath, $"\"{generatedName}\",", "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
        }

        private static void MoveFile(string sourcePath, string destinationPath) => 
            File.Move(sourcePath, destinationPath);
        private static void MoveMetaFile(string sourcePath, string destinationPath) => 
            File.Move($"{sourcePath}.meta", $"{destinationPath}.meta");
        
        private static void MoveFolderWithContent(string sourcePath, string destinationPath)
        {
            Directory.Move(sourcePath, destinationPath);
            
            var supportedExtensions = new[] { ".cs", ".asmdef" };
            Directory.EnumerateFiles(destinationPath, "*", SearchOption.AllDirectories)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file)))
                .ToList()
                .ForEach(file =>
                {
                    var generatedNamespace = GenerateNamespace(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar);
                    if (IsAsmdefFile(file))
                        ChangeOrAddLine(file, generatedNamespace, "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
                    
                    else if (IsCsFile(file))
                        ChangeOrAddLine(file,generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
                    
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