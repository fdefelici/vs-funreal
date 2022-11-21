using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FUnreal
{
    public class XFilesystem
    {
        public static readonly char PathSeparatorChar = Path.DirectorySeparatorChar;
        public static readonly string PathSeparatorStr = $"{PathSeparatorChar}";


        //TODO: Make Async and Parallel
        public static bool DeepCopy(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath)) return false;
            Directory.CreateDirectory(targetPath); //Make sure target directory exists in case sourcePath contains only files
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetDir = dirPath.Replace(sourcePath, targetPath);
                    Directory.CreateDirectory(targetDir);
                }

                foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string targetFile = filePath.Replace(sourcePath, targetPath);
                    File.Copy(filePath, targetFile, true);
                }
            }
            catch (Exception) 
            {
                return false;       
            }
            return true;
        }


        public static void DeepCopy(string sourcePath, string targetPath, PlaceHolderReplaceStrategy strategy)
        {
          
                //List<string> dirs = new List<string>();
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetDir = dirPath.Replace(sourcePath, targetPath);
                    targetDir = strategy.HandlePath(targetDir);

                    Directory.CreateDirectory(targetDir);
                    //dirs.Add(targetDir);
                }

                List<string> files = new List<string>();
                //Copy all the files & Replaces any files with the same name
                foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string targetFile = filePath.Replace(sourcePath, targetPath);
                    targetFile = strategy.HandlePath(targetFile);

                    File.Copy(filePath, targetFile, true);
                    strategy.HandleFileContent(targetFile);

                    //files.Add(targetFile);
                }

                //return new Tuple<List<string>, List<string>>(dirs, files);
        }

        public static async Task DeepCopyAsync(string sourcePath, string targetPath, PlaceHolderReplaceStrategy strategy)
        {
            await Task.Run(() => {
                //List<string> dirs = new List<string>();
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetDir = dirPath.Replace(sourcePath, targetPath);
                    targetDir = strategy.HandlePath(targetDir);

                    Directory.CreateDirectory(targetDir);
                    //dirs.Add(targetDir);
                }

                List<string> files = new List<string>();
                //Copy all the files & Replaces any files with the same name
                foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string targetFile = filePath.Replace(sourcePath, targetPath);
                    targetFile = strategy.HandlePath(targetFile);

                    File.Copy(filePath, targetFile, true);
                    strategy.HandleFileContent(targetFile);

                    //files.Add(targetFile);
                }

                //return new Tuple<List<string>, List<string>>(dirs, files);
            });
        }

        public static bool HasExtension(string file, params string[] exts)
        {
            return HasExtension(file, exts.ToList<string>());
        }
        public static bool HasExtension(string file, List<string> extensions)
        {
            //string fileExt = Path.GetExtension(file);
            //return extensions.Contains(fileExt);
            //More like EndsWith to satifsfy file named after ".Build.cs"
            //not really an "extension" in the very sense, but make this method smooth to use.
            string fileName = Path.GetFileName(file);
            foreach(string each in extensions)
            {
                if (fileName.EndsWith(each)) return true;
            }
            return false;
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

        public static string PathCombine(string first, params string[] parts)
        {
            string combined = first;

            foreach (var part in parts)
            {
                combined = Path.Combine(combined, part);
            }

            //ensure all separators are OS specific
            return PathFixSeparator(combined, true);
        }

        public static string PathFixSeparator(string path, bool removeLast=false)
        {
            string sepStr = $"{Path.DirectorySeparatorChar}";
            string result = path.Replace("/", sepStr);
            result = result.Replace("\\", sepStr);
            if (removeLast && result.EndsWith(sepStr))
            {
                result = result.Substring(0, result.Length - sepStr.Length);
            }
            return result;
        }

        public static string PathSubtract(string totalPath, string subPath, bool keepSubPathLastDir = false)
        {
            string sepStr = $"{Path.DirectorySeparatorChar}";
            string totFixed = PathFixSeparator(totalPath, true);
            string subFixed = PathFixSeparator(subPath, true);

            string result = totFixed.Replace(subFixed, "");
            if (result.StartsWith(sepStr))
            {
                result = result.Substring(sepStr.Length);
            }
            if (result.EndsWith(sepStr))
            {
                result = result.Substring(0, result.Length - sepStr.Length);
            }


            if (keepSubPathLastDir)
            {
                result = PathCombine(Path.GetFileName(subPath), result);
            }
            return result;
        }

        public static string GetFilenameNoExt(string path, bool considerMultipleDotsExtension = false)
        {
            if (!considerMultipleDotsExtension)
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                string fileName = Path.GetFileName(path);
                string[] parts = fileName.Split('.');
                return parts[0];
            }
        }

        public static bool DeleteDir(string basePath)
        {
            if (!Directory.Exists(basePath)) return false;
            try { 
                Directory.Delete(basePath, true);
            } catch (Exception) { return false; }
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

            try { 
                File.Move(filePath, newPath);
            } catch (Exception)
            {
                return null;
            }
            return newPath;
        }

        public static string RenameDir(string sourcePath, string dirName)
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

            bool succeded = DeepCopy(sourcePath, destPath);
            if (!succeded)
            {
                DeleteDir(destPath); //Cleanup partial copy
                return null;
            }
            
            succeded = DeleteDir(sourcePath);
            if (!succeded) return null;

            return destPath;
        }

        public static string PathParent(string path, int levels = 1)
        {
            string result = path;
            for (int i=0; i < levels; ++i)
            {
                result = Path.GetDirectoryName(result);
            }
            return result;
        }

        public static string GetLastPathToken(string path)
        {
            return Path.GetFileName(path);
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }


        public static List<string> FindFiles(string path, bool recursive, Func<string, bool> filter)
        {
            return FindFiles(path, recursive, "*.*", filter);
        }

        public static List<string> FindFiles(string path, bool recursive, string searchPattern)
        {
            return FindFiles(path, recursive, searchPattern, filePath => true);
        }

        public static List<string> FindFilesStoppingDepth(string path, string searchPattern)
        {
            List<string> result = new List<string>();

            Queue<string> dirToVisit = new Queue<string>();
            dirToVisit.Enqueue(path);

            while (dirToVisit.Any())
            {
                string currentDir = dirToVisit.Dequeue();

                var listFound = FindFiles(currentDir, false, searchPattern);
                if (listFound.Count > 0)
                {
                    result.AddRange(listFound);
                }
                else
                {
                    var subDirs = FindDirectories(currentDir);
                    subDirs.ForEach(dirToVisit.Enqueue);
                }
            }
            return result;
        }


        public static List<string> FindFiles(string path, bool recursive, string searchPattern, Func<string, bool> filter)
        {
            List<string> result = new List<string>();
            if (!DirectoryExists(path)) return result;

            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;  
            var filePaths = Directory.EnumerateFiles(path, searchPattern, searchMode);

            foreach(string each in filePaths)
            {
                if (filter(each))
                {
                    result.Add(each);
                }
            }
            return result;
        }


        public static List<string> FindFilesAtLevel(string path, int dirLevel, string searchPattern)
        {
            if (dirLevel == 0) return Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly).ToList();

            var dirsCurrLevel = Directory.EnumerateDirectories(path);
            List<string> files = new List<string>();
            foreach(var dir in dirsCurrLevel)
            {
                var found = FindFilesAtLevel(dir, dirLevel - 1, searchPattern);
                files.AddRange(found);
            }
            return files;
        }

        public static string FindFile(string path, bool recursive, string searchPattern)
        {
            if (!DirectoryExists(path)) return null;

            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filePaths = Directory.EnumerateFiles(path, searchPattern, searchMode);
            if (filePaths.Any()) return filePaths.ElementAt(0);
            return null;
        }

        public static string FindFile(string path, bool recursive, string searchPattern, Func<string, bool> filter)
        {
            if (!DirectoryExists(path)) return null;

            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filePaths = Directory.EnumerateFiles(path, searchPattern, searchMode);

            foreach (string each in filePaths)
            {
                if (filter(each))
                {
                    return each;
                }
            }
            return null;
        }


        public static List<string> FindDirectories(string fullPath, bool recursive = false)
        {
            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (!Directory.Exists(fullPath)) return new List<string>();
            return Directory.EnumerateDirectories(fullPath, "*", searchMode).ToList();
        }

        public static string PathChild(string path, int level=1)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            string result = "";
            bool first = true;
            for(int i=level; i<parts.Length; i++)
            {
                if (first)
                {
                    result = parts[i];
                    first = false;
                } else
                {
                    result = XFilesystem.PathCombine(result, parts[i]);
                }
            }
            return result;
        }

        public static void FileCopy(string sourceFilePath, string destFilePath) 
        {
            if (!FileExists(sourceFilePath)) return;

            string destBasePath = PathParent(destFilePath);
            Directory.CreateDirectory(destBasePath);

            File.Copy(sourceFilePath, destFilePath, true);
        }

        public static string PathToUnixStyle(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
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
            } catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void CreateFile(string filePath)
        {
            XFilesystem.WriteFile(filePath, "");
        }

        public static string ChangeFilePathExtension(string filePath, string newExtention)
        {
            return Path.ChangeExtension(filePath, newExtention);
        }

        public static string FilePathChangeNameWithExt(string filePath, string newFileNameWithExt)
        {
            string basePath = PathParent(filePath);
            return PathCombine(basePath, newFileNameWithExt);
        }

        public static string ChangeFilePathName(string filePath, string newFileName)
        {
            string ext = Path.GetExtension(filePath);
            string basePath = PathParent(filePath);
            return PathCombine(basePath, $"{newFileName}{ext}");
        }

        public static async Task<List<string>> DirectoryFilesAsync(string dirPath, string searchPattern, bool recurse)
        {
            return await Task.Run(() =>
            {
                var array = Directory.GetFiles(dirPath, searchPattern, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                return array.ToList();
            });

            
        }

        public static string GetFileNameWithExt(string filePath)
        {
            return GetLastPathToken(filePath);
        }

        public static string[] PathSplit(string path)
        {
            return path.Split(Path.DirectorySeparatorChar);
        }

        public static int PathCount(string path)
        {
            return PathSplit(path).Count();
        }

        public static bool IsChildPath(string childPath, string parentPath)
        {
            return childPath.Contains(parentPath);
        }

        public static string ChangePathBase(string fullPath, string basePath, string newBasePath)
        {
            return fullPath.Replace(basePath, newBasePath);
        }

        public static bool CreateDir(string fullPath)
        {
            if (DirectoryExists(fullPath)) return true; 

            try
            {
                Directory.CreateDirectory(fullPath);
                return true;
            } catch (Exception) 
            {
                return false;                
            }
        }

        public static string ChangeDirName(string fullPath, string newDirName)
        {
            string parent = XFilesystem.PathParent(fullPath);
            return XFilesystem.PathCombine(parent, newDirName);
        }
    }
}
