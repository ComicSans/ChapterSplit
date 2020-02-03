using ChapterSplit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ChapterSplit.CLIOperations
{
    internal class MergeOperation : ICLIOperation
    {
        private readonly string Verbose = "";
        private IEnumerable<string> SourceFiles { get; }

        public MergeOperation(IEnumerable<String> SourceFiles, bool verbose = false)
        {
            this.SourceFiles = SourceFiles;
            if (!verbose)
            {
                this.Verbose = $"-hide_banner -loglevel panic -nostats";
            }
        }

        public void Run()
        {
            // ffmpeg -i '.\1.m4b' -i '.\2.m4b' -filter_complex concat=n=2:v=0:a=1 -f mp4 -vn -y out.m4b
            Console.OutputEncoding = Encoding.UTF8;
            string Inputs = "";
            foreach (var inputFile in this.SourceFiles)
            {
                Console.WriteLine($"Processing: '{inputFile}'");
                var serializedOutput = MediaHelper.GetMediaInfo(inputFile);
                var chapters = serializedOutput.Chapters;
                var formatDescriptions = serializedOutput.Format;
                string filePath = $"{inputFile}";
                var fileExtension = Path.GetExtension(filePath);

                Inputs += $"-i \"{inputFile}\" ";

                // todo: set title to album
                // todo: add chaptermarks
            }

            var pExtract = new Process
            {
                StartInfo =
                            {
                                UseShellExecute = false,
                                FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe",
                                Arguments = $"{Inputs} -filter_complex concat=n=2:v=0:a=1 -map_metadata 0 -map_chapters 0 -vn -f mp4 -y {this.Verbose} target.m4b"
                            }
            };
            pExtract.Start();
            // wait for end of extraction.
            pExtract.WaitForExit();
            pExtract.Dispose();
        }
    }
}