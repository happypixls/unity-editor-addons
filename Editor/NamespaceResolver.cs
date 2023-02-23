#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public enum FileType
{
    None,
    CSharpScript = 0,
    InterfaceFile = 1,
    EnumFile = 2,
    Asemdef = 3,
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
            { FileType.Asemdef, GenerateAssemblyDefinition },
        };
    
    private static string GenerateNamespace(string metaFilePath)
    {
        var SegmentedPath = $"{Path.GetDirectoryName(metaFilePath)}".Split(new[] { '\\' }, StringSplitOptions.None);

        var GeneratedNamespace = "";
        var FinalNamespace = "";

        // In case of placing the class at the root of a folder such as (Editor, Scripts, etc...)  
        if (SegmentedPath.Length <= 2)
            FinalNamespace = UnityEditor.EditorSettings.projectGenerationRootNamespace;
        else
        {
            // Skipping the Assets folder and a single subfolder (i.e. Scripts, Editor, Plugins, etc...)
            for (var i = 2; i < SegmentedPath.Length; i++)
            {
                GeneratedNamespace +=
                    i == SegmentedPath.Length - 1
                        ? SegmentedPath[i]        // Don't add '.' at the end of the namespace
                        : SegmentedPath[i] + "."; 
            }
            
            FinalNamespace = EditorSettings.projectGenerationRootNamespace + "." + GeneratedNamespace;
        }
        
        return FinalNamespace;
    }

    private static void OnWillCreateAsset(string metaFilePath)
    {
        var FileName = Path.GetFileNameWithoutExtension(metaFilePath);

        if (!metaFilePath.EndsWith(".cs"))
            return;
        
        if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace))
        {
            Debug.LogError("Root Namespace is not set in project settings, Namespace resolver is aborted.");
            return;
        }
        
        Debug.Log("CALLING CREATION OF SCRIPTS");
        
        FileTemplatesGenerators[CurrentlyCreatedFile].Invoke(metaFilePath, FileName);
    }

    private static void GenerateCSharpMonobehaviourScript(string metaFilePath, string FileName)
    {
        var FinalNamespace = GenerateNamespace(metaFilePath);
        var ActualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{FileName}";
        var MyTemplate =
            File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpMonobehaviourTemplate.cs.txt");
        var NewContent = MyTemplate.Replace("#NAMESPACE#", Regex.Replace(FinalNamespace, @"\b \b", ""))
            .Replace("#SCRIPTNAME#", FileName.Substring(0, FileName.IndexOf('.')));

        if (MyTemplate != NewContent)
        {
            File.WriteAllText(ActualFile, NewContent);
            AssetDatabase.Refresh();
        }
    }

    private static void GenerateCSharpInterface(string metaFilePath, string FileName)
    {
        var FinalNamespace = GenerateNamespace(metaFilePath);
        var ActualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{FileName}";
        var MyTemplate =
            File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpInterfaceTemplate.cs.txt");
        var NewContent = MyTemplate.Replace("#NAMESPACE#", Regex.Replace(FinalNamespace, @"\b \b", ""))
            .Replace("#SCRIPTNAME#", FileName.Substring(0, FileName.IndexOf('.')));

        if (MyTemplate != NewContent)
        {
            File.WriteAllText(ActualFile, NewContent);
            AssetDatabase.Refresh();
        }
    }
    
    private static void GenerateCSharpEnum(string metaFilePath, string FileName)
    {
        var FinalNamespace = GenerateNamespace(metaFilePath);
        var ActualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{FileName}";
        var MyTemplate =
            File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/CSharpEnumTemplate.cs.txt");
        var NewContent = MyTemplate.Replace("#NAMESPACE#", Regex.Replace(FinalNamespace, @"\b \b", ""))
            .Replace("#SCRIPTNAME#", FileName.Substring(0, FileName.IndexOf('.')));

        if (MyTemplate != NewContent)
        {
            File.WriteAllText(ActualFile, NewContent);
            AssetDatabase.Refresh();
        }
    }
    
    private static void GenerateAssemblyDefinition(string metaFilePath, string FileName)
    {
        var FinalNamespace = GenerateNamespace(metaFilePath);
        var ActualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{FileName}";
        var MyTemplate =
            File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/Templates/AssemblyDefinition.cs.txt");
        var NewContent = MyTemplate.Replace("#NAMESPACE#", Regex.Replace(FinalNamespace, @"\b \b", ""))
            .Replace("#SCRIPTNAME#", FileName.Substring(0, FileName.IndexOf('.')));

        if (MyTemplate != NewContent)
        {
            File.WriteAllText(ActualFile, NewContent);
            AssetDatabase.Refresh();
        }
    }
    
    public static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        if (!sourcePath.EndsWith(".cs"))
        {
            Debug.Log("Moving none cs file" + sourcePath);
            
            if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.Directory))
            {
                Directory.Move(sourcePath, destinationPath);
                Directory.EnumerateFiles(destinationPath).ToList().ForEach(file => 
                    ChangeOrAddLine(file, 
                        GenerateNamespace(file + ".meta"), "namespace"));
            }
            else
            {
                File.Move(sourcePath, destinationPath);
            }
            
            File.Move(sourcePath + ".meta", destinationPath + ".meta");
            return AssetMoveResult.DidMove;
        }
        
        ChangeOrAddLine(sourcePath, GenerateNamespace(destinationPath + ".meta"), "namespace");
        
        File.Move(sourcePath, destinationPath);
        File.Move(sourcePath + ".meta", destinationPath + ".meta");
            
        return AssetMoveResult.DidMove;
    }
    
    //The worst thing about this approach is that we need to read the whole file which is unnecessary...
    private static void ChangeOrAddLine(string filePath, string newLine, string beginningTextLine = "")
    {
        using var Fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        using var Sr = new StreamReader(Fs);
        using var Sw = new StreamWriter(Fs);
        
        var Lines = Sr.ReadToEnd().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        Fs.Position = 0;
        if (beginningTextLine != "")
        {
            for (var i = 0; i < Lines.Count; i++)
            {
                var SplitLine = Lines[i].Split(' ');
                if (SplitLine[0] == beginningTextLine)
                {
                    SplitLine[1] = newLine;
                    Lines[i] = SplitLine[0] + " " + SplitLine[1];
                    break;
                }
            }
        }
        Sw.Write(string.Join("\r\n", Lines));
        Fs.SetLength(Fs.Position);
    }
}
#endif