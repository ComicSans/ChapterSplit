using CommandLine;
using System.Collections.Generic;

namespace AudioChapterSplit
{
    public class Options
    {
        [Option('i', "Input", HelpText = "MP3 files to split (separate them with \":\")", Separator = ':')]
        public IEnumerable<string> SourceFiles { get; set; }

        [Option('v', "Verbose", HelpText = "Show debug output")]
        public bool Verbose { get; set; }

    }

}
