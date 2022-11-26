using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            if (parts.Length == 0) return first;
            
            string combined = first;
            int startIndex = 0;
            if (string.IsNullOrEmpty(combined))
            {
                combined = parts[0];
                startIndex = 1;
            }
            
            for(int i = startIndex; i < parts.Count(); ++i)
            {
                string part = parts[i];
                //combined = Path.Combine(combined, part);  No used anymore because try to combine 'C:' with 'some' result in 'C:some' instead of 'C:\some'
                if (string.IsNullOrEmpty(part)) continue;
                combined = $"{combined}{PathSeparatorChar}{part}";
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
            if (string.IsNullOrEmpty(path)) return string.Empty;

            string result = path;
            for (int i=0; i < levels; ++i)
            {
                result = Path.GetDirectoryName(result);
                if (string.IsNullOrEmpty(result)) return string.Empty;
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

        public static string GetFilenameExtension(string filePath, bool considerMultipleDotExt = false)
        {
            if (!considerMultipleDotExt)
            {
                return Path.GetExtension(filePath);
            }
            else
            {
                string fileNameWithExt = GetFileNameWithExt(filePath);
                string fileNameNoExt = GetFilenameNoExt(filePath, considerMultipleDotExt);

                string result = fileNameWithExt.Substring(fileNameNoExt.Length);
                return result;
            }
        }

        public static string ChangeFilePathName(string filePath, string newFileName, bool considerMultipleDotExt = false)
        {
            string ext = GetFilenameExtension(filePath, considerMultipleDotExt);
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

        public static bool IsChildPath(string childPath, string parentPath, bool samePathIsValid = false)
        {
            var childParts = PathSplit(childPath);
            var parentParts = PathSplit(parentPath);

            int childCount = childParts.Count();
            int parentCount = parentParts.Count();
            if (parentCount > childCount) return false;
            if (!samePathIsValid && parentCount == childCount) return false;

            for(int i=0; i<parentParts.Count(); i++) 
            { 
                if (parentParts[i] != childParts[i]) return false;
            }
            return true;
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

        public static string GetRoot(string path)
        {
            return GetPartAt(path, 0);
        }

        public static string GetPartAt(string path, int index)
        {
            var parts = path.Split(PathSeparatorChar);
            if (index < 0 || index >= parts.Length) return string.Empty;

            return parts[index];
        }

        public static string SelectCommonBasePath(List<string> paths)
        {
            if (paths.Count == 0) return string.Empty;
            if (paths.Count == 1) return paths[0];

            bool found = false;
            int level = 0;

            string reference = paths[0];
            while (!found)
            {
                string refPart = XFilesystem.GetPartAt(reference, level);
                if (refPart == string.Empty)
                {
                    //reached the end of the reference string. Ref string is the common path
                    break;
                }
                
                foreach (var each in paths)
                {
                    string childPart = XFilesystem.GetPartAt(each, level);
                    if (childPart != refPart)
                    {
                        level--; //compesate level++ at end of the while
                        found = true;
                        break;
                    }
                }
                level++;
            }

            string selectedBasePath = XFilesystem.PathBase(reference, level);
            return selectedBasePath;
        }

        public static string PathBase(string path, int count)
        {
            var parts = PathSplit(path);
            if (count >= parts.Length) return path;

            string result = "";
            bool first = true;
            for (int i = 0; i < count; i++)
            {
                if (first)
                {
                    result = parts[i];
                    first = false;
                }
                else
                {
                    result = XFilesystem.PathCombine(result, parts[i]);
                }
            }
            return result;
        }
        
        /*
        public static List<string> FindDirectories(string fullPath, bool recursive = false)
        {
            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (!Directory.Exists(fullPath)) return new List<string>();
            return Directory.EnumerateDirectories(fullPath, "*", searchMode).ToList();
        }
        */

        private static object FindEmptyFolderLock = new object();

        public static async Task<List<string>> FindEmptyFoldersAsync(string basePath, params string[] otherBasePaths)
        {
            List<string> result = new List<string>();
            var stack = new Stack<IEnumerable<string>>();

            var first = new List<string>();
            if (DirectoryExists(basePath)) first.Add(basePath);
            foreach(var other in otherBasePaths)
            {
                if (DirectoryExists(other)) first.Add(other);
            }

            stack.Push(first);
            while(stack.Any())
            {
                var nextDirs = stack.Pop();

                await Task.Run(() =>
                {
                    foreach(var eachDir in nextDirs)
                    {
                        try { 
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

                        } catch(Exception e)
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
                for(int j=0; j < count; j++)
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
            foreach(var each in filtered)
            {
                if (!FolderHasAnyFile(each, true))
                {
                    result.Add(each);
                }
            }
            return result;
        }

        public static bool IsParentPath(string possibleParent, string possibleChild, bool samePathIsConsideredParent = false)
        {
            var delta = PathSubtract(possibleChild, possibleParent);
            if (string.IsNullOrEmpty(delta) && !samePathIsConsideredParent) return false;
            if (delta.Length == possibleChild.Length) return false;
            return true;
        }

        public static bool FolderHasAnyFile(string folderPath, bool recursive)
        {
            return FindFile(folderPath, recursive, "*.*") != null;
        }

        public static string ToPath(string[] parts, int startIndex=0)
        {
            string result = string.Empty;
            if (startIndex < 0 || startIndex >= parts.Length) return result;

            bool first = true;
            for (int i = startIndex; i < parts.Length; i++)
            {
                if (first)
                {
                    result = parts[i];
                    first = false;
                }
                else
                {
                    result = XFilesystem.PathCombine(result, parts[i]);
                }
            }
            return result;
        }
    }
}
