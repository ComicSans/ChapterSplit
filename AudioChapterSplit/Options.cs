using CommandLine;
using System.Collections.Generic;

namespace AudioChapterSplit
{
    
    public class Options
    {
        private readonly IEnumerable<string> _sourceFiles;
        private readonly bool _verbose;
        private readonly bool _merge;

        public Options(IEnumerable<string> SourceFiles, bool Verbose, bool Merge)
        {
            this._sourceFiles = SourceFiles;
            this._verbose = Verbose;
            this._merge = Merge;
        }

        [Option('i', "Input", HelpText = "MP3/M4B/M4A files to split (separated by \" \", no wildcards allowed)")]
        public IEnumerable<string> SourceFiles { get { return this._sourceFiles; }  }

        [Option('v', "Verbose", HelpText = "Show debug output")]
        public bool Verbose { get { return this._verbose; } }

        [Option('m', "Merge", HelpText = "Merge input files")]
        public bool Merge { get { return this._merge; } }


    }

}
