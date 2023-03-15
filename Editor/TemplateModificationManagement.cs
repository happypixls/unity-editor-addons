using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HappyPixels.EditorAddons
{
    public class TemplateModificationManagement : AssetModificationProcessor
    {
        private static Dictionary<FileType, Action<string, string>> FileTemplatesGenerators { get; } = 
            new() 
            {
                { FileType.CSharpScript, TemplateGenerationManagement.GenerateCSharpMonobehaviourScript },
                { FileType.CSharpClass, TemplateGenerationManagement.GenerateCSharpClass },
                { FileType.InterfaceFile, TemplateGenerationManagement.GenerateCSharpInterface },
                { FileType.EnumFile, TemplateGenerationManagement.GenerateCSharpEnum },
                { FileType.Asmdef, TemplateGenerationManagement.GenerateAssemblyDefinition },
            };
        
        private static void ChangeCSFileContent(string filePath, string destinationPath)
        {
            var generatedNamespace = NamespaceResolver.GenerateNamespace($"{destinationPath}.meta");
            ChangeOrAddLine(filePath, generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
        }

        private static void ChangeAsmdefContent(string filePath, string destinationPath)
        {
            var generatedName = NamespaceResolver.GenerateNamespace($"{destinationPath}.meta");
            ChangeOrAddLine(filePath, $"\"{generatedName}\",", "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
        }

        private static void FileModifier(string fileContent)
        {
            var generatedNamespace = NamespaceResolver.GenerateNamespace(Path.GetDirectoryName(fileContent) + Path.DirectorySeparatorChar);
            if (FileUtilities.IsAsmdefFile(fileContent))
                ChangeOrAddLine(fileContent, generatedNamespace, "\"name\"", ':', (s1, s2, c) => $"{s1}{c} \"{s2}\",");
                    
            else if (FileUtilities.IsCSFile(fileContent))
                ChangeOrAddLine(fileContent,generatedNamespace, "namespace", ' ', (s1, s2, c) => $"{s1}{c}{s2}");
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
        
        private static void OnWillCreateAsset(string metaFilePath)
        {
            if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace))
            {
                Debug.LogError("Root Namespace is not set in project settings, Namespace resolver is aborted.");
                return;
            }
            
            var fileName = Path.GetFileNameWithoutExtension(metaFilePath);
            
            if (FileUtilities.IsCSFile(fileName))
            {
                switch (TemplateGenerationManagement.CurrentlyCreatedFile)
                {
                    case FileType.CSharpScript:
                        FileTemplatesGenerators[FileType.CSharpScript].Invoke(metaFilePath, fileName);
                        break;
                    default:
                        FileTemplatesGenerators[TemplateGenerationManagement.CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                        break;
                }
                TemplateGenerationManagement.CurrentlyCreatedFile = FileType.CSharpScript;
                return;
            }
            
            if (FileUtilities.IsAsmdefFile(fileName))
            {
                TemplateGenerationManagement.CurrentlyCreatedFile = FileType.Asmdef;
                FileTemplatesGenerators[TemplateGenerationManagement.CurrentlyCreatedFile].Invoke(metaFilePath, fileName);
                
                TemplateGenerationManagement.CurrentlyCreatedFile = FileType.CSharpScript; //This is set in order not to conflict with cs interface, enums or POC creation
            }
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (FileUtilities.IsDirectory(sourcePath))
            {
                FileUtilities.MoveFolderWithContent(sourcePath, destinationPath, FileModifier);
                FileUtilities.MoveMetaFile(sourcePath, destinationPath);
                return AssetMoveResult.DidMove;
            }

            if (FileUtilities.IsCSFile(sourcePath)) 
                ChangeCSFileContent(sourcePath, destinationPath);

            if (FileUtilities.IsAsmdefFile(sourcePath))
                ChangeAsmdefContent(sourcePath, destinationPath);

            FileUtilities.MoveFile(sourcePath, destinationPath);
            FileUtilities.MoveMetaFile(sourcePath, destinationPath);

            return AssetMoveResult.DidMove;
        }
    }
}