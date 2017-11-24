// -----------------------------------------------------------------------
//  <copyright file="DirectoryUtil.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.FileSystem
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class DirectoryUtil
    {
        public DirectoryUtil(string fullPath)
        {
            Path = fullPath;
            if (fullPath.ElementAt(fullPath.Length - 1) != '\\')
                Path += "\\";
        }

        public DirectoryUtil(string name, bool rootAsCurrentDir) : this(name)
        {
            if (rootAsCurrentDir)
                Path = Current.Path + name + @"\";
        }

        public DirectoryUtil(string name, string path) : this(name)
        {
            Path = path + name;
        }

        public DirectoryUtil(string name, DirectoryUtil pathDir) : this(name)
        {
            Path = System.IO.Path.Combine(pathDir.Path, name);
        }

        public DirectoryUtil() { }

        public static DirectoryUtil Current { get; set; }
        public string Name { get; }
        public string Path { get; }

        public List<DirectoryUtil> GetDirectories()
        {
            var list = new List<DirectoryUtil>();
            foreach (var item in Directory.GetDirectories(Path))
                list.Add(new DirectoryUtil(item));
            return list;
        }

        public List<string> GetFiles() => Directory.GetFiles(Path).ToList();

        public DirectoryUtil Create()
        {
            Directory.CreateDirectory(Path);
            return this;
        }

        public bool Exists() => Directory.Exists(Path);

        public void Open() => Process.Start(Path);

        public DirectoryUtil Select()
        {
            Current = this;
            return this;
        }
    }
}