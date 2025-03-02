using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FUnreal
{
    //XFilesystem APIs which involve IO
    public partial class XFilesystem
    {
        private const string LONG_PATH_PREFIX = @"\\?\";



        public static string ToLongPath(string path)
        {
            if (path.StartsWith(LONG_PATH_PREFIX)) return path;
            return $"{LONG_PATH_PREFIX}{path}";
        }

        public static string ToNormalPath(string path)
        {
            if (path == null) return null;
            if (!path.StartsWith(LONG_PATH_PREFIX)) return path;
            return path.Substring(LONG_PATH_PREFIX.Length);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirDeepCopy(string sourcePath, string targetPath)
        {
            return DirDeepCopy(sourcePath, targetPath, new NullDeepCopyVisitor());
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static async Task<bool> DirDeepCopyAsync(string sourcePath, string targetPath)
        {
            return await DirDeepCopyAsync(sourcePath, targetPath, new NullDeepCopyVisitor());
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirDeepCopy(string sourcePath, string targetPath, IDeepCopyVisitor visitor)
        {
            if (!DirExists(sourcePath)) return false;
            //Make sure target directory exists in case sourcePath contains only files
            if (!DirCreate(targetPath)) return false;
           
            try
            {
                //Now Create all of the directories
                var dirs = FindDirsEnum(sourcePath, true);
                foreach (string dirPath in dirs)
                {
                    string targetDir = dirPath.Replace(sourcePath, targetPath);
                    targetDir = visitor.HandlePath(targetDir);

                    if (!DirCreate(targetDir)) return false;
                }
     
                var files = FindFilesEnum(sourcePath, true, "*.*");
                //Copy all the files & Replaces any files with the same name
                foreach (string filePath in files)
                {
                    string targetFile = filePath.Replace(sourcePath, targetPath);
                    targetFile = visitor.HandlePath(targetFile);

                    //File.Copy(ToLongPath(filePath), ToLongPath(targetFile), true);
                    if (!FileCopy(filePath, targetFile)) return false;
                    visitor.HandleFileContent(targetFile);
                }
            }
            catch (Exception e) 
            {
                XDebug.Erro(e.Message);
                XDebug.Erro(e.StackTrace);
                return false; 
            }
            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static async Task<bool> DirDeepCopyAsync(string sourcePath, string targetPath, IDeepCopyVisitor visitor)
        {
            return await Task.Run(() =>
            {
                return DirDeepCopy(sourcePath, targetPath, visitor);
            });
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static string FileRead(string file)
        {
            try
            {
                string lp = ToLongPath(file);   
                return File.ReadAllText(lp);
            } catch(Exception)
            {
                return null;
            }
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static IEnumerable<string> FileReadLines(string file)
        {
            try
            {
                string lp = ToLongPath(file);
                return File.ReadLines(lp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileWrite(string file, string contents)
        {   
            file = ToLongPath(file);
            string basePath = PathParent(file);

            if (!DirCreate(basePath)) return false;

            try
            {
                File.WriteAllText(file, contents);
            } catch (Exception) 
            {
                return false;
            }
            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirDelete(string basePath)
        {
            if (!DirExists(basePath)) return false;
            try
            {
                string lp = ToLongPath(basePath);
                Directory.Delete(lp, true);
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirIsEmpty(string basePath)
        {
            if (!DirExists(basePath)) return false;
            return !Directory.EnumerateFileSystemEntries(ToLongPath(basePath)).Any();
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static string FileRename(string filePath, string newFileNameNoExt)
        {
            string basePath = PathParent(filePath);
            string fileExt = GetFilenameExtension(filePath);
            string newPath = PathCombine(basePath, newFileNameNoExt + fileExt);

            try
            {
                //NOTE: File.Move doesn't seem to suffer of the issue related to Directory.Move
                //      So can use Move, instead of Copy + Delete
                File.Move(ToLongPath(filePath), ToLongPath(newPath));
            }
            catch (Exception)
            {
                return null;
            }
            return newPath;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static string FileRenameWithExt(string filePath, string newFileNameWithExt)
        {
            string basePath = PathParent(filePath);
            string newPath = PathCombine(basePath, newFileNameWithExt);

            if (!FileExists(filePath)) return null;
            if (FileExists(newPath)) return null;

            try
            {
                File.Move(ToLongPath(filePath), ToLongPath(newPath));
            }
            catch (Exception e)
            {
                XDebug.Erro(e.Message);
                XDebug.Erro(e.StackTrace);
                return null;
            }
            return newPath;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static async Task<string> RenameDirAsync(string sourcePath, string dirName)
        {
            string basePath = PathParent(sourcePath);
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

            bool succeded = await DirDeepCopyAsync(sourcePath, destPath);
            if (!succeded)
            {
                DirDelete(destPath); //Cleanup partial copy
                return null;
            }

            succeded = DirDelete(sourcePath);
            if (!succeded) return null;

            return destPath;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileExists(string path)
        {
            string lp = ToLongPath(path);
            return File.Exists(lp);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileExists(string path, bool recurse, string searchPattern)
        {
            var filePaths = FindFilesEnum(path, recurse, searchPattern);
            return filePaths.Any();
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileExists(string path, bool recurse, string searchPattern, Func<string, bool> predicate)
        {
            var filePaths = FindFilesEnum(path, recurse, searchPattern);
            return filePaths.Any(predicate);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static string FindFile(string path, bool recursive, string searchPattern)
        {
            var files = FindFilesEnum(path, recursive, searchPattern);
            if (files.Any()) return ToNormalPath(files.ElementAt(0));
            return null;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static string FindFile(string path, bool recursive, string searchPattern, Func<string, bool> predicate)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            return ToNormalPath(filePaths.FirstOrDefault(predicate));
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static IEnumerable<string> FindFilesEnum(string path, bool recursive, string searchPattern)
        {
            if (!DirExists(path)) return new List<string>();
            string lp = ToLongPath(path);
            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return XFilesystemEnumerable.AdaptToNormal(Directory.EnumerateFiles(lp, searchPattern, searchMode));
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static IEnumerable<string> FindFilesEnum(string path, bool recursive, string searchPattern, Func<string, bool> predicate)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            return filePaths.Where(each => predicate(each));
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static async Task<IEnumerable<string>> FindFilesEnumAsync(string dirPath, bool recurse, string searchPattern)
        {
            return await Task.Run(() =>
            {
                return FindFilesEnum(dirPath, recurse, searchPattern);
            });
        }

        /// <remarks>Protected againts Long Path</remarks>
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
                    var subDirs = FindDirsEnum(currentDir);
                    foreach(var each in subDirs) dirToVisit.Enqueue(each);
                }
            }
            return result;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static IEnumerable<string> FindDirsEnum(string fullPath, bool recursive = false)
        {
            SearchOption searchMode = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (!DirExists(fullPath)) return new List<string>();
            string lp = ToLongPath(fullPath);
            return XFilesystemEnumerable.AdaptToNormal(Directory.EnumerateDirectories(lp, "*", searchMode));
        }

        public static async Task<IEnumerable<string>> FindDirsEnumAsync(string dirPath, bool recurse = false)
        {
            return await Task.Run(() =>
            {
                return FindDirsEnum(dirPath, recurse);
            });
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirExists(string dirPath)
        {
            var lp = ToLongPath(dirPath);
            return Directory.Exists(lp);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileDelete(string filePath)
        {
            if (!FileExists(filePath)) return false;

            try
            {   
                string lp = ToLongPath(filePath);
                File.Delete(lp);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirCreate(string fullPath)
        {
            string lp = ToLongPath(fullPath);

            if (DirExists(lp)) return true;

            try
            {
                Directory.CreateDirectory(lp);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileCreate(string filePath)
        {
            return FileWrite(filePath, "");
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileCopy(string sourceFilePath, string destFilePath)
        {
            if (!FileExists(sourceFilePath)) return false;

            string destBasePath = PathParent(destFilePath);
            if (!DirCreate(destBasePath)) return false;

            try
            {
                File.Copy(ToLongPath(sourceFilePath), ToLongPath(destFilePath), true);
            } catch(Exception)
            {
                return false;
            }
            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static async Task<List<string>> FindEmptyFoldersAsync(string basePath, params string[] otherBasePaths)
        {
            List<string> result = new List<string>();
            var stack = new Stack<IEnumerable<string>>();

            var first = new List<string>();
            if (DirExists(basePath))
            {   
                first.Add(ToLongPath(basePath));
            }
            foreach (var other in otherBasePaths)
            {
                if (DirExists(other)) first.Add(ToLongPath(other));
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
                                var eachSubDirs = XFilesystemEnumerable.AdaptToLong(Directory.EnumerateDirectories(eachDir));
                                if (eachSubDirs.Any())
                                {   
                                    stack.Push(eachSubDirs);
                                }
                            }
                            else
                            {
                                result.Add(XFilesystem.ToNormalPath(eachDir));
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

        /// <remarks>Protected againts Long Path</remarks>
        public static void FilesForEach(string path, bool recursive, string searchPattern, Action<string> action)
        {
            var filePaths = FindFilesEnum(path, recursive, searchPattern);
            foreach (var each in filePaths) action(each);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirContainsAnyFile(string folderPath, bool recursive)
        {
            return FindFile(folderPath, recursive, "*.*") != null;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool FileIsLocked(string filePath)
        {
            try
            {
                string lp = ToLongPath(filePath);
                var stream = File.OpenWrite(lp);
                stream.Close();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool DirContainsAnyFileLocked(string dirPath, bool recursive, string searchPattern, out string firstFileLocked)
        {
            firstFileLocked = FindFile(dirPath, recursive, searchPattern, filePath =>
            {
                if (FileIsLocked(filePath)) return true;
                else return false;
            });

            if (firstFileLocked == null) return false;
            return true;
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static JObject JsonFileRead(string filePath)
        {
            var text = FileRead(filePath);
            if (text == null) return null;
            return JObject.Parse(text);
        }

        /// <remarks>Protected againts Long Path</remarks>
        public static bool JsonFileWrite(string filePath, JToken json)
        {
            string basePath = PathParent(filePath);
            if (!DirCreate(basePath)) return false;

            string lp = ToLongPath(filePath);

            try
            {
                StreamWriter sw = new StreamWriter(lp);
                var wr = new JsonTextWriter(sw);
                wr.Formatting = Formatting.Indented;
                wr.IndentChar = '\t';
                wr.Indentation = 1;

                json.WriteTo(wr);

                sw.Flush();
                wr.Close();
            } catch (Exception ex) {
                XDebug.Erro(ex.Message);
                XDebug.Erro(ex.StackTrace);
                return false;
            }

            return true;
        }
    }
}
