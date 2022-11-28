using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FUnreal
{
    //XFilesystem APIs which involve playing Path
    public partial class XFilesystem
    {
        public static readonly char PathSeparatorChar = Path.DirectorySeparatorChar;
        public static readonly string PathSeparatorStr = $"{PathSeparatorChar}";

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

        public static string PathToUnixStyle(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
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
