using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataTools.CSTools
{
    public abstract class CodeParserBase<TMarker, TList>
        where TMarker : IMarker<TMarker, TList>, new()
        where TList : IMarkerList<TMarker>, new()
    {
        protected string[] lines = null;
        protected string text = null;

        protected int preambleTo = 0;

        protected TList markers = new TList();
        protected AtomicGenerationInfo<TMarker, TList> mfile = null;

        protected List<string> lastErrors = new List<string>();

        protected string filename = null;

        protected string[] outputFiles = new string[0];
        protected List<string> unrecognizedWords = new List<string>();

        /// <summary>
        /// Gets or sets the output path for files generated from this parser.
        /// </summary>
        public virtual string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the list of output files generated from this parser.
        /// </summary>
        public virtual string[] OutputFiles => outputFiles;

#if EXPERIMENTAL

        /// <summary>
        /// Gets or sets the 'Interfaces' directory name (default is 'Contracts')
        /// </summary>
        public virtual string InterfaceDirName { get; set; } //= CSAppOptions.Instance.InterfaceFolderName;

        /// <summary>
        /// Gets or sets the 'Classes' directory name (default is none)
        /// </summary>
        public virtual string ClassDirName { get; set; } //= CSAppOptions.Instance.ClassFolderName;

        /// <summary>
        /// Gets or stets the 'Enums' directory name (default is 'Enums')
        /// </summary>
        public virtual string EnumDirName { get; set; } //= CSAppOptions.Instance.EnumFolderName;

        /// <summary>
        /// Gets or stets the 'Structs' directory name (default is 'Structs')
        /// </summary>
        public virtual string StructDirName { get; set; } //= CSAppOptions.Instance.StructFolderName;

        /// <summary>
        /// Gets or sets a value indicating that files containing different types of objects will go in different subdirectories beneath the selected output directory.
        /// </summary>
        public virtual bool SeparateDirs { get; set; } //= CSAppOptions.Instance.UseSeparateFolders;

#else

        /// <summary>
        /// Gets or sets the 'Interfaces' directory name (default is 'Contracts')
        /// </summary>
        public virtual string InterfaceDirName { get; set; } = CSAppOptions.Instance.InterfaceFolderName;

        /// <summary>
        /// Gets or sets the 'Classes' directory name (default is none)
        /// </summary>
        public virtual string ClassDirName { get; set; } = CSAppOptions.Instance.ClassFolderName;

        /// <summary>
        /// Gets or stets the 'Enums' directory name (default is 'Enums')
        /// </summary>
        public virtual string EnumDirName { get; set; } = CSAppOptions.Instance.EnumFolderName;

        /// <summary>
        /// Gets or stets the 'Structs' directory name (default is 'Structs')
        /// </summary>
        public virtual string StructDirName { get; set; } = CSAppOptions.Instance.StructFolderName;

        /// <summary>
        /// Gets or sets a value indicating that files containing different types of objects will go in different subdirectories beneath the selected output directory.
        /// </summary>
        public virtual bool SeparateDirs { get; set; } = CSAppOptions.Instance.UseSeparateFolders;
#endif

        /// <summary>
        /// Gets a value indicating if the last parse was successful.
        /// </summary>
        public virtual bool ParseSuccess { get; protected set; } = false;

        /// <summary>
        /// Gets a value indicating that the parser loaded the file lazily and has not yet parsed it.
        /// </summary>
        public virtual bool IsLazyLoad { get; protected set; } = false;

        /// <summary>
        /// Gets the name of the original source file.
        /// </summary>
        public virtual string Filename
        {
            get => filename;
            protected set => filename = value;
        }

        /// <summary>
        /// Gets a list of the last errors.
        /// </summary>
        public virtual IReadOnlyList<string> Errors
        {
            get => lastErrors;
        }

        /// <summary>
        /// Gets the current list of markers that were parsed from source text.
        /// </summary>
        public virtual TList Markers => markers;

        /// <summary>
        /// Gets the end position of the preamble in the source text.
        /// </summary>
        public virtual int PreambleTo => preambleTo;

        /// <summary>
        /// Gets the source code text.
        /// </summary>
        public virtual string Text => text;

        /// <summary>
        /// Gets the source code text broken into lines.
        /// </summary>
        public virtual string[] Lines => lines;

        /// <summary>
        /// Gets a list of all unrecognized words in this file.
        /// </summary>
        public IReadOnlyList<string> UnrecognizedWords => unrecognizedWords;

        protected object SyncRoot { get; } = new object();

        /// <summary>
        /// Load and optionally parse a code file.
        /// </summary>
        /// <param name="filename">The valid relative or absolute pathname to the source file.</param>
        /// <param name="lazy">True to load the file without parsing it.</param>
        /// <exception cref="FileNotFoundException"></exception>
        public abstract void LoadFile(string filename, bool lazy = false);

        /// <summary>
        /// Gets the markers filtered for commit.
        /// </summary>
        /// <returns></returns>
        public abstract TList GetMarkersForCommit();

        /// <summary>
        /// Write the results of the refactor and reorganized file structure to disk.
        /// </summary>
        /// <param name="path">The destination path.</param>
        /// <param name="separateDirs">True to use separate directories for different item types.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public virtual bool OutputMarkers(string path = null, bool? separateDirs = null)
        {
            if (separateDirs is bool bo)
            {
                SeparateDirs = bo;
            }

            if (path != null)
            {
                OutputPath = path;
            }
            else
            {
                path = OutputPath;
            }

            if (!Directory.Exists(OutputPath)) throw new DirectoryNotFoundException(OutputPath);

            TList seen = new TList();

            if (mfile == null) return false;
            if (markers == null) return false;

            var mks = GetMarkersForCommit();

            foreach (var marker in mks)
            {
                if (seen.Contains(marker)) continue;

                var mlist = new TList() { marker };

                var name = marker.Name;
                var ns = marker.Namespace;
                var kind = marker.Kind;

                var mf = new AtomicGenerationInfo<TMarker, TList>()
                {
                    Lines = mfile.Lines,
                    Markers = mlist,
                    PreambleBegin = mfile.PreambleBegin,
                    PreambleEnd = mfile.PreambleEnd
                };

                var file = OutputFile<TMarker, TList>.NewFile(path, mf, lines, SeparateDirs, this);
                file.Write();
            }

            return true;
        }

        /// <summary>
        /// Reread the file from the disk and reparse its contents.
        /// </summary>
        /// <remarks>
        /// This method will unset <see cref="IsLazyLoad"/>.
        /// </remarks>
        public virtual void Refresh()
        {
            lock (SyncRoot)
            {
                IsLazyLoad = false;
                Parse(File.ReadAllText(Filename));
            }
        }

        public override string ToString()
        {
            return Filename;
        }

        /// <summary>
        /// Parse the given source code text.
        /// </summary>
        /// <param name="text">The source code text to parse.</param>
        /// <returns>True if successful.</returns>
        protected abstract bool Parse(string text);

        /// <summary>
        /// Set the parent <see cref="AtomicGenerationInfo{TMarker, TList}"/> object to the specified marker and its descendents.
        /// </summary>
        /// <param name="marker">The marker to modify.</param>
        /// <param name="file">The parent file object.</param>
        protected void SetAtomicFile(TMarker marker, AtomicGenerationInfo<TMarker, TList> file)
        {
            marker.AtomicSourceFile = file;

            if (marker.Children != null)
            {
                foreach (var child in marker.Children)
                {
                    SetAtomicFile(child, file);
                }
            }
        }
    }
}