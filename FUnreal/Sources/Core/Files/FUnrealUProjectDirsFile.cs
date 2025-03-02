using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows.Media.Imaging;

namespace FUnreal
{
    /// <summary>
    /// Example of .uprojectdirs file
    /// 
    /// ; These folders will be searched 1 level deep in order to find projects
    /// ; UnrealBuildTool will store the following information:
    /// ; - Project name
    /// ; - Location of project
    /// ; - Whether it has code or not
    /// ; - TargetNames contains at the project location
    /// ;
    /// ./
    /// Engine/Source/
    /// Engine/Programs/
    /// 
    /// </summary>
    public class FUnrealUProjectDirsFile
    {
        private string _filePath;

        public List<string> Paths { get; private set; }

        public FUnrealUProjectDirsFile(string filePath) 
        { 
            _filePath = filePath;
            Paths = new List<string>();

            LoadFile();
        }

        private void LoadFile()
        {
            var lines = XFilesystem.FileReadLines(_filePath);
            foreach (var line in lines)
            {
                bool isComment = line.StartsWith(";");
                if (isComment) continue;
                bool isEmpty = line.Trim().Length == 0;
                if (isEmpty) continue;

                Paths.Add(line);
            }

        }
    }
}
