using ChapterSplit;
using ChapterSplit.ffprobeOutput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ChapterSplit.CLIOperations
{
    internal class MergeOperation : ICLIOperation
    {
        private readonly string Verbose = "";
        private string workingDir;
        private Format formatDescriptions;
        private double totalDuration = 0;

        private IEnumerable<string> SourceFiles { get; }

        public MergeOperation(IEnumerable<String> SourceFiles, bool verbose = false)
        {
            this.SourceFiles = SourceFiles;
            if (!verbose)
            {
                this.Verbose = $"-hide_banner -loglevel panic -nostats";
            }

            string filePath = $"{SourceFiles.ElementAt(0)}";
            workingDir = Path.GetDirectoryName(filePath);

            this.formatDescriptions = MediaHelper.GetMediaInfo(filePath).Format;
            this.PrepareMetadataFile(this.formatDescriptions);
        }

        public void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            string Inputs = "";
            var numberOfFiles = 0;

            foreach (var inputFile in this.SourceFiles)
            {
                Console.WriteLine($"Processing: '{inputFile}'");
                var serializedOutput = MediaHelper.GetMediaInfo(inputFile);
                var formatDescriptions = serializedOutput.Format;

                this.WriteMetadataFile(formatDescriptions);

                Inputs += $"-i \"{inputFile}\" ";
                numberOfFiles += 1;
            }

            Console.WriteLine($"Merging files into \"{this.formatDescriptions.Tags.Artist} - {this.formatDescriptions.Tags.Album}.m4b\"");

            var pMerge = new Process
            {
                StartInfo =
                            {
                                UseShellExecute = false,
                                WorkingDirectory = this.workingDir,
                                FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe",
                                Arguments = $"-i \".\\metadata.txt\" {Inputs}-filter_complex concat=n={numberOfFiles}:v=0:a=1 -map_metadata 0 -map_chapters 0 -vn -c:a libfdk_aac -vbr 3 -vn -movflags +faststart -f mp4 -y {this.Verbose} ~tmp.m4b"
                            }
            };
            pMerge.Start();
            pMerge.WaitForExit();
            pMerge.Dispose();

            var pMetadata = new Process
            {
                StartInfo =
                            {
                                UseShellExecute = false,
                                WorkingDirectory = this.workingDir,
                                FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe",
                                Arguments = $"-i \".\\~tmp.m4b\" -i \".\\cover.png\" -map 0:0 -map 1:0 -c copy -id3v2_version 3 -metadata:s:v title=\"Album cover\" -metadata:s:v comment=\"Cover (front)\" -y -f mp4 {this.Verbose} \"{this.formatDescriptions.Tags.Artist} - {this.formatDescriptions.Tags.Album}.m4b\""
                            }
            };
            pMetadata.Start();
            pMetadata.WaitForExit();
            pMetadata.Dispose();
            this.DeleteTmpFiles();
        }

        private void WriteLinesToFile(string[] lines)
        {
            var file = new System.IO.StreamWriter($"{this.workingDir}\\metadata.txt", true);
            foreach (string line in lines)
            {
                file.WriteLine(line);
            }
            file.Close();
        }

        private void DeleteTmpFiles()
        {
            if (File.Exists($"{this.workingDir}\\metadata.txt"))
            {
                File.Delete($"{this.workingDir}\\metadata.txt");
            }
            if (File.Exists($"{this.workingDir}\\~tmp.m4b"))
            {
                File.Delete($"{this.workingDir}\\~tmp.m4b");
            }
        }

        private void PrepareMetadataFile(Format formatDescriptions)
        {
            this.DeleteTmpFiles();
            string[] lines = { ";FFMETADATA1", $"title={formatDescriptions.Tags.Album}", $"artist={formatDescriptions.Tags.Artist}", $"genre={formatDescriptions.Tags.Genre}", "" };
            this.WriteLinesToFile(lines);
        }

        private void WriteMetadataFile(Format formatDescriptions)
        {
            var duration = Convert.ToDouble(formatDescriptions.Duration, new CultureInfo("en-US"));

            string[] lines = { "[CHAPTER]", "TIMEBASE=1/1", $"START={this.totalDuration}", $"END={this.totalDuration + duration}", $"TITLE={formatDescriptions.Tags.Title}" };
            this.WriteLinesToFile(lines);

            this.totalDuration += duration;
        }
    }
}