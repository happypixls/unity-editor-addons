#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//If there would be more than one keyword to replace, add a Dictionary
public class NamespaceResolver : UnityEditor.AssetModificationProcessor 
{
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
            
            FinalNamespace = UnityEditor.EditorSettings.projectGenerationRootNamespace + "." + GeneratedNamespace;
        }
        
        return FinalNamespace;
    }
    
    private static void OnWillCreateAsset(string metaFilePath)
    {
        if (string.IsNullOrEmpty(UnityEditor.EditorSettings.projectGenerationRootNamespace))
        {
            Debug.LogError("Root Namespace is not set in project settings, Namespace resolver is aborted.");
            return;
        }
        
        var FileName = Path.GetFileNameWithoutExtension(metaFilePath);

        if (!FileName.EndsWith(".cs"))
            return;

        var FinalNamespace = GenerateNamespace(metaFilePath);
        var ActualFile = $"{Path.GetDirectoryName(metaFilePath)}\\{FileName}";
        var MyTemplate = File.ReadAllText("Packages/com.happypixels.editoraddons/Editor/MonobehaviourTemplate.txt");
        var NewContent = MyTemplate.Replace("#NAMESPACE#", Regex.Replace(FinalNamespace, @"\b \b", ""))
            .Replace("#SCRIPTNAME#", FileName.Substring(0, FileName.IndexOf('.')));

        if (MyTemplate != NewContent)
        {
            File.WriteAllText(ActualFile, NewContent);
            AssetDatabase.Refresh();
        }
    }

    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;
        if (!sourcePath.EndsWith(".cs"))
        {
            File.Move(sourcePath, destinationPath);
            File.Move(sourcePath + ".meta", destinationPath + ".meta");
            return assetMoveResult;
        }
        
        ChangeOrAddLine(sourcePath, GenerateNamespace(destinationPath + ".meta"), "namespace");
        
        File.Move(sourcePath, destinationPath);
        File.Move(sourcePath + ".meta", destinationPath + ".meta");
        
        return assetMoveResult;
    }
    
    //The worst thing about this approach is that we need to read the whole file which is unnecessary...
    private static void ChangeOrAddLine(string filePath, string newLine, string beginningTextLine = "")
    {
        using (var Fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
        using (var Sr = new StreamReader(Fs))
        using (var Sw = new StreamWriter(Fs))
        {
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
}
#endif