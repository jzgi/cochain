﻿using System.IO;

namespace Greatbone.Core
{
    ///
    /// The environment for a particular folder.
    ///
    public class FolderContext
    {
        readonly string name;

        public FolderContext(string name)
        {
            this.name = name;
        }

        public Service Service { get; internal set; }

        public string Name => name;

        public AuthorizeAttribute[] Authorizes { get; internal set; }

        public UiAttribute Ui { get; internal set; }

        public bool IsVar { get; internal set; }

        public int Level { get; internal set; }

        public Folder Parent { get; internal set; }

        public string Directory { get; internal set; }

        public string GetFilePath(string file)
        {
            return Path.Combine(Directory, file);
        }
    }
}