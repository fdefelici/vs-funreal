using FUnreal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FUnrealTest
{
    public static class TestUtils
    {
        public static string BinBasePath => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        public static string AbsPath(params string[] parts)
        {
            return PathCombine(BinBasePath, parts);
        }

        public static string PathCombine(string first, params string[] parts)
        {
            return XFilesystem.PathCombine(first, parts);
        }

        public static string MakeDir(string dirPath, params string[] parts)
        {
            string path = PathCombine(dirPath, parts);
            if (!XFilesystem.DirCreate(path)) throw new Exception("Cannot create directory");
            return path;
        }

        public static void WriteFile(string filePath, string contents)
        {
            XFilesystem.FileWrite(filePath, contents);
        }

        public static void DeleteDir(string basePath, params string[] parts)
        {
            string path = XFilesystem.PathCombine(basePath, parts);
            XFilesystem.DirDelete(path);
        }

        public static string MakeFile(string dirPath, params string[] parts)
        {
            string filePath = PathCombine(dirPath, parts);
            if (!XFilesystem.FileCreate(filePath)) return null;
            return filePath;
        }

        public static void DeepCopy(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static bool ExistsDir(string first, params string[] parts)
        {
            string dirPath = XFilesystem.PathCombine(first, parts);
            return Directory.Exists(dirPath);
        }

        public static bool ExistsFile(string first, params string[] parts)
        {
            string filePath = XFilesystem.PathCombine(first, parts);
            return XFilesystem.FileExists(filePath);
        }

        public static string ReadFile(string first, params string[] parts)
        {
            string filePath = XFilesystem.PathCombine(first, parts);
            if (!ExistsFile(filePath)) return null;
            return File.ReadAllText(filePath);
        }

        public static string PathParent(string path, int level=1)
        {
            return XFilesystem.PathParent(path, level);
        }

        public static string RenameFile(string filePath, string name)
        {
            return XFilesystem.FileRename(filePath, name);
        }

        internal static string RenameFolder(string sourcePath, string dirName)
        {
            string basePath = Path.GetDirectoryName(sourcePath);
            string destPath = PathCombine(basePath, dirName);

            Directory.Move(sourcePath, destPath);

            return destPath;
        }

        internal static void DeleteFile(string file)
        {
            XFilesystem.FileDelete(file);
        }

        private static Dictionary<string, FileStream> lockedFiles = new Dictionary<string, FileStream>();

        public static void FileLock(string filePath)
        {
            lock (lockedFiles)
            {
                if (!ExistsFile(filePath)) throw new Exception("File must exists!");
                if (lockedFiles.ContainsKey(filePath)) return;

                var stream = File.OpenWrite(XFilesystem.ToLongPath(filePath));
                lockedFiles[filePath] = stream;
            }
        }

        public static void FileUnlock(string filePath)
        {
            lock (lockedFiles)
            {
                if (!ExistsFile(filePath)) throw new Exception("File must exists!");
                if (!lockedFiles.ContainsKey(filePath)) return;

                var stream = lockedFiles[filePath];
                stream.Close();

                lockedFiles.Remove(filePath);
            }
        }
    }
}