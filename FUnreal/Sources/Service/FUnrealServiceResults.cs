using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class AFUnrealServiceResult
    {
        public static T Create<T>(bool value) where T : AFUnrealServiceResult, new() 
        {
            T obj = new T();
            obj.IsSuccess = value;
            return obj;
        }

        public static implicit operator bool(AFUnrealServiceResult value)
        {
            return value.IsSuccess;
        }

        public bool IsSuccess { get; internal set; }
    }

    public class FUnrealServiceSimpleResult 
        : AFUnrealServiceResult
    {
        public static implicit operator FUnrealServiceSimpleResult(bool value)
        {
            return Create<FUnrealServiceSimpleResult>(value);
        }
    }

    public class FUnrealServiceSourceClassResult
        : AFUnrealServiceResult
    {
        public static implicit operator FUnrealServiceSourceClassResult(bool value)
        {
            return Create<FUnrealServiceSourceClassResult>(value);
        }

        public string HeaderPath { get; set; }
        public string SourcePath { get; set; }
    }

    public class FUnrealServiceFileResult
       : AFUnrealServiceResult
    {
        public static implicit operator FUnrealServiceFileResult(bool value)
        {
            return Create<FUnrealServiceFileResult>(value);
        }

        public string FilePath { get; set; }
    }

    public class FUnrealServiceFilesResult
       : AFUnrealServiceResult
    {
        

        public static implicit operator FUnrealServiceFilesResult(bool value)
        {
            return Create<FUnrealServiceFilesResult>(value);
        }

        public List<string> AllPaths { get; set; }
        public List<string> AllParentPaths { get; set; }

        public List<string> FilePaths { get; set; }
        public List<string> DirPaths { get; set; }
    }

    public class FUnrealServiceModuleResult
      : AFUnrealServiceResult
    {
        public static implicit operator FUnrealServiceModuleResult(bool value)
        {
            return Create<FUnrealServiceModuleResult>(value);
        }

        public string BuildFilePath { get; set; }
    }

    public class FUnrealServicePluginResult
     : AFUnrealServiceResult
    {
        public static implicit operator FUnrealServicePluginResult(bool value)
        {
            return Create<FUnrealServicePluginResult>(value);
        }

        public string DescrFilePath { get; set; }
    }
}
