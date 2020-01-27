using CommandLine;
using System.Collections.Generic;

namespace AudioChapterSplit
{
    
    public class Options
    {
        private readonly IEnumerable<string> _sourceFiles;
        private readonly bool _debug;
        private readonly bool _merge;

        public Options(IEnumerable<string> SourceFiles, bool Debug, bool Merge)
        {
            this._sourceFiles = SourceFiles;
            this._debug = Debug;
            this._merge = Merge;
        }

        [Option('i', "Input", HelpText = "MP3/M4B/M4A files to split (separated by \" \", no wildcards allowed)")]
        public IEnumerable<string> SourceFiles { get { return this._sourceFiles; }  }

        [Option('d', "Debug", HelpText = "Show debug output")]
        public bool Debug { get { return this._debug; } }

        [Option('m', "Merge", HelpText = "Merge input files")]
        public bool Merge { get { return this._merge; } }


    }

}
