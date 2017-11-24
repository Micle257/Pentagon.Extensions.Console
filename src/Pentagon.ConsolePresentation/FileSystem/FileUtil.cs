// -----------------------------------------------------------------------
//  <copyright file="FileUtil.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

using IOFile = System.IO.File;

namespace Pentagon.Utilities.Console.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FileUtil
    {
        public static bool ResetConfig = false;
        public static FileChangedEventHandle OnChangedMethod;

        public FileUtil() { }

        public FileUtil(string name, FileType type)
        {
            Name = name;
            Folder = DirectoryUtil.Current;
            Type = type;
            Path = Folder.Path + "\\" + Name + "." + Type.ToString().ToLower();
            Values = new List<string[]>();
            FileContent = new List<string>();
            DefaultFile = null;
            Changed += OnChangedMethod;
        }

        public FileUtil(DirectoryUtil folder, string name, FileType type) : this(name, type)
        {
            Folder = folder;
        }

        public delegate void FileChangedEventHandle(FileUtil obj, FileEvent operation);

        public event FileChangedEventHandle Changed;

        //// Enums
        public enum FileType
        {
            Txt,
            Db,
            Ini,
            Log
        }

        public string Name { get; }
        public string Path { get; }
        public FileType Type { get; }
        public DirectoryUtil Folder { get; }

        public List<string[]> Values { get; }
        public List<string> FileContent { get; set; }
        public string[] DefaultFile { get; set; }

        //// Methods
        public virtual void Load()
        {
            Create();
            FileContent = ReadLines().ToList();
            foreach (var item in FileContent)
                Values.Add(item.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

            OnChanged(this, FileEvent.Loaded);
        }

        public virtual void SortFile()
        {
            var wholeFile = IOFile.ReadAllLines(Path);
            var newFile = wholeFile.ToList();
            newFile.RemoveAll(s => string.IsNullOrEmpty(s));
            newFile.Sort();
            IOFile.WriteAllLines(Path, newFile);
        }

        public void Dispose()
        {
            Values.Clear();
            FileContent.Clear();
        }

        public void Create() => Create(DefaultFile);

        public IEnumerable<string> ReadLines() => IOFile.ReadLines(Path);

        public void Delete()
        {
            IOFile.Delete(Path);
            OnChanged(this, FileEvent.Deleted);
        }

        public void Backup()
        {
            var original = IOFile.ReadAllLines(Path);
            IOFile.WriteAllLines(Path + ".bak", original);
            OnChanged(this, FileEvent.Backup);
        }

        public void Encrypt()
        {
            // Cryptology.EncryptFile(this.Path, Folder.Path + Name + ".encrypted." + Type, "pentagon");
        }

        public void Decrypt()
        {
            // Cryptology.DecryptFile(Folder.Path + Name + ".encrypted." + Type, this.Path, "pentagon");
        }

        public void WriteLine(int lineNum, string line)
        {
            var wholeFile = FileContent;
            wholeFile[lineNum] = line;
            IOFile.WriteAllLines(Path, wholeFile);
        }

        public void WriteLine(string line)
        {
            var wholeFile = FileContent;
            wholeFile.Add(line);
            IOFile.WriteAllLines(Path, wholeFile);
        }

        public void WriteLineReverse(int lineNum, string line)
        {
            var wholeFile = FileContent;
            wholeFile.Reverse();
            wholeFile[lineNum] = line;
            wholeFile.Reverse();
            IOFile.WriteAllLines(Path, wholeFile);
        }

        protected virtual void OnChanged(FileUtil obj, FileEvent operation)
        {
            if (Changed != null)
                Changed(obj, operation);
        }

        protected void Create(string[] lines)
        {
            if (!IOFile.Exists(Path))
            {
                using (var file = IOFile.Create(Path))
                {
                    if (lines != null)
                        IOFile.WriteAllLines(Path, lines);
                    OnChanged(this, FileEvent.Created);
                }
            }
        }
    }
}