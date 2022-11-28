using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FUnreal
{
    //XFilesystem APIs which involve IO
    public partial class XFilesystem
    {
        public static bool DeepCopy(string sourcePath, string targetPath)
        {
            return DeepCopy(sourcePath, targetPath, new NullDeepCopyVisitor());
        }

        public static async Task<bool> DeepCopyAsync(string sourcePath, string targetPath)
        {
            return await DeepCopyAsync(sourcePath, targetPath, new NullDeepCopyVisitor());
        }

        public static bool DeepCopy(string sourcePath, string targetPath, IDeepCopyVisitor visitor)
        {
            if (!Directory.Exists(sourcePath)) return false;
            Directory.CreateDirectory(targetPath); //Make sure target directory exists in case sourcePath contains only files

            try
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetDir = dirPath.Replace(sourcePath, targetPath);
                    targetDir = visitor.HandlePath(targetDir);

                    Directory.CreateDirectory(targetDir);
                }

                List<string> files = new List<string>();
                //Copy all the files & Replaces any files with the same name
                foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string targetFile = filePath.Replace(sourcePath, targetPath);
                    targetFile = visitor.HandlePath(targetFile);

                    File.Copy(filePath, targetFile, true);
                    visitor.HandleFileContent(targetFile);
                }
            }
            catch (Exception) { return false; }
            return true;
        }

        public static async Task<bool> DeepCopyAsync(string sourcePath, string targetPath, IDeepCopyVisitor visitor)
        {
            return await Task.Run(() =>
            {
                return DeepCopy(sourcePath, targetPath, visitor);
            });
        }

        public static string ReadFile(string file)
        {
            return File.ReadAllText(file);
        }

        public static void WriteFile(string file, string contents)
        {
            string basePath = Path.GetDirectoryName(file);
            Directory.CreateDirectory(basePath);
            File.WriteAllText(file, contents);
        }

        public static bool DeleteDir(string basePath)
        {
            if (!Directory.Exists(basePath)) return false;
            try
            {
                Directory.Delete(basePath, true);
            }
            catch (Exception) { return false; }
            return true;
        }

        public static bool IsEmptyDir(string basePath)
        {
            if (!Directory.Exists(basePath)) return false;
            if (Directory.GetFiles(basePath).Length != 0) return false;
            if (Directory.GetDirectories(basePath).Length != 0) return false;
            return true;
        }

        public static JObject ReadJsonFile(string filePath)
        {
            return JObject.Parse(ReadFile(filePath));
        }

        public static void WriteJsonFile(string filePath, JToken json)
        {
            string basePath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(basePath);

            StreamWriter sw = new StreamWriter(filePath);
            var wr = new JsonTextWriter(sw);
            wr.Formatting = Formatting.Indented;
            wr.IndentChar = '\t';
            wr.Indentation = 1;

            json.WriteTo(wr);

            sw.Flush();
            wr.Close();
        }

        public static string RenameFileName(string filePath, string newFileNameNoExt)
        {
            string basePath = Path.GetDirectoryName(filePath);
            string fileExt = Path.GetExtension(filePath);
            string newPath = PathCombine(basePath, newFileNameNoExt + fileExt);

            try
            {
                //NOTE: File.Move doesn't seem to suffer of the issue related to Directory.Move
                //      So can use Move, instead of Copy + Delete
                File.Move(filePath, newPath);
            }
            catch (Exception)
            {
                return null;
            }
            return newPath;
        }

        public static string RenameFileNameWithExt(string filePath, string newFileNameWithExt)
        {
            string basePath = Path.GetDirectoryName(filePath);
            string newPath = PathCombine(basePath, newFileNameWithExt);

            try
            {
                File.Move(filePath, newPath);
            }
            catch (Exception)
            {
                return null;
            }
            return newPath;
        }

        public static async Task<string> RenameDirAsync(string sourcePath, string dirName)
        {
            string basePath = Path.GetDirectoryName(sourcePath);
            string destPath = PathCombine(basePath, dirName);

            //NOTE: If a file was opened with Notepad++ and trying to move the folder 
            //      seems to generate "Access Denied exception"
            //      Maybe a bug of internal move api https://stackoverflow.com/questions/43325099/c-sharp-directory-move-access-denied-error
            // As a workaround, converting Move operation in Copy/Delete.
            /*
            try
            {
                Directory.Move(sourcePath, destPath);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                return null;
            }
            return destPath;
            */

            bool succeded = await DeepCopyAsync(sourcePath, destPath);
            if (!succeded)
            {
                DeleteDir(destPath); //Cleanup partial copy
                return null;
            }

            succeded = DeleteDir(sourcePath);
            if (!succeded) return null;

            return destPath;
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool FileExists(string path, bool recurse, string searchPattern, Func<string, bool> predicate)
        {
            //return FindFile(path, recurse, searchPattern, predicate) != null;
            var filePaths = FindFilesEnum(path, recurse, searchPattern);
            return filePaths.Any(predicate);
        }

        public static string FindFile(string path, bool recursive, string searchPattern)
        {
            var files = FindFilesEnum(path, recursive, searchPattern);
            if (files.Any()) return files.ElementAt(0);
            return null;
        }

        public static string FindFile(string path, bool recursive, string searchPattern, Func<string, bool> predicate)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            return filePaths.FirstOrDefault(predicate);
        }

        public static IEnumerable<string> FindFilesEnum(string path, bool recursive, string searchPattern)
        {
            if (!DirectoryExists(path)) return new List<string>();

            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.EnumerateFiles(path, searchPattern, searchMode);
        }

        public static IEnumerable<string> FindFilesEnum(string path, bool recursive, string searchPattern, Func<string, bool> predicate)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            return filePaths.Where(each => predicate(each));
        }

        public static void FilesForEach(string path, bool recursive, string searchPattern, Action<string> action)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            foreach(var each in filePaths) action(each);
        }


        public static async Task<IEnumerable<string>> FindFilesEnumAsync(string dirPath, bool recurse, string searchPattern)
        {
            return await Task.Run(() =>
            {
                return FindFilesEnum(dirPath, recurse, searchPattern);
            });
        }



        public static List<string> FindFilesStoppingDepth(string path, string searchPattern)
        {
            List<string> result = new List<string>();

            Queue<string> dirToVisit = new Queue<string>();
            dirToVisit.Enqueue(path);

            while (dirToVisit.Any())
            {
                string currentDir = dirToVisit.Dequeue();

                var listFound = FindFilesEnum(currentDir, false, searchPattern);
                if (listFound.Any())
                {
                    result.AddRange(listFound);
                }
                else
                {
                    var subDirs = FindDirectoriesEnum(currentDir);
                    foreach(var each in subDirs) dirToVisit.Enqueue(each);
                }
            }
            return result;
        }

        public static IEnumerable<string> FindDirectoriesEnum(string fullPath, bool recursive = false)
        {
            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (!Directory.Exists(fullPath)) return new List<string>();
            return Directory.EnumerateDirectories(fullPath, "*", searchMode);
        }

        public static bool DirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        public static bool DeleteFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool CreateDir(string fullPath)
        {
            if (DirectoryExists(fullPath)) return true;

            try
            {
                Directory.CreateDirectory(fullPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void CreateFile(string filePath)
        {
            XFilesystem.WriteFile(filePath, "");
        }

        public static void FileCopy(string sourceFilePath, string destFilePath)
        {
            if (!FileExists(sourceFilePath)) return;

            string destBasePath = PathParent(destFilePath);
            Directory.CreateDirectory(destBasePath);

            File.Copy(sourceFilePath, destFilePath, true);
        }

        public static async Task<List<string>> FindEmptyFoldersAsync(string basePath, params string[] otherBasePaths)
        {
            List<string> result = new List<string>();
            var stack = new Stack<IEnumerable<string>>();

            var first = new List<string>();
            if (DirectoryExists(basePath)) first.Add(basePath);
            foreach (var other in otherBasePaths)
            {
                if (DirectoryExists(other)) first.Add(other);
            }

            stack.Push(first);
            while (stack.Any())
            {
                var nextDirs = stack.Pop();

                await Task.Run(() =>
                {
                    foreach (var eachDir in nextDirs)
                    {
                        try
                        {
                            var eachContents = Directory.EnumerateFileSystemEntries(eachDir);
                            if (eachContents.Any())
                            {
                                var eachSubDirs = Directory.EnumerateDirectories(eachDir);
                                if (eachSubDirs.Any())
                                {
                                    stack.Push(eachSubDirs);
                                }
                            }
                            else
                            {
                                result.Add(eachDir);
                            }

                        }
                        catch (Exception e)
                        {
                            XDebug.Erro("Failed scan empty dirs for {0}", eachDir);
                            XDebug.Info(e.Message);
                        }
                    }
                });
            }
            return result;
        }

        public static List<string> SelectFolderPathsNotContainingAnyFile(List<string> paths)
        {
            var result = new List<string>();

            //Remove path that are parents of others
            var filtered = new List<string>();
            int count = paths.Count();
            for (int i = 0; i < count; i++) //should improve algo if paths are sorted 
            {
                var current = paths[i];
                bool isParent = false;
                for (int j = 0; j < count; j++)
                {
                    if (i == j) continue;

                    var other = paths[j];
                    if (IsParentPath(current, other))
                    {
                        isParent = true;
                        break;
                    }
                }

                if (!isParent && !filtered.Contains(current))
                {
                    filtered.Add(current);
                }
            }

            //Scan for the one who contains recursive at least one file
            foreach (var each in filtered)
            {
                if (!FolderHasAnyFile(each, true))
                {
                    result.Add(each);
                }
            }
            return result;
        }

        public static bool FileIsLocked(string filePath)
        {
            try
            {
                var stream = File.OpenWrite(filePath);
                stream.Close();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static bool DirectoryHasAnyFileLocked(string dirPath, bool recursive, string searchPattern, out string firstFileLocked)
        {
            firstFileLocked = FindFile(dirPath, recursive, searchPattern, filePath =>
            {
                if (FileIsLocked(filePath)) return true;
                else return false;
            });

            if (firstFileLocked == null) return false;
            return true;
        }
    }
}
