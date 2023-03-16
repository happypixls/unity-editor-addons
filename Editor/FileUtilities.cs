using System;
using System.IO;
using System.Linq;

namespace HappyPixels.EditorAddons
{
    internal static class FileUtilities
    {
        internal static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        internal static bool IsCSFile(string path) => path.EndsWith(".cs");
        internal static bool IsAsmdefFile(string path) => path.EndsWith(".asmdef");
        internal static void MoveFile(string sourcePath, string destinationPath) => 
            File.Move(sourcePath, destinationPath);
        
        internal static void MoveMetaFile(string sourcePath, string destinationPath) => 
            File.Move($"{sourcePath}.meta", $"{destinationPath}.meta");
        
        internal static void MoveFolderWithContent(string sourcePath, string destinationPath, Action<string> contentModifier)
        {
            Directory.Move(sourcePath, destinationPath);
            
            var supportedExtensions = new[] { ".cs", ".asmdef" };
            Directory.EnumerateFiles(destinationPath, "*", SearchOption.AllDirectories)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file)))
                .ToList()
                .ForEach(f => contentModifier?.Invoke(f));
        }

    }
}