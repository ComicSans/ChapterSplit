using ChapterSplit.ffprobeOutput;
using ChapterSplit.CLIOperations;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ChapterSplit
{
    internal class SplitOperation : ICLIOperation
    {
        private static int count = 0;
        private readonly string inputFile;
        private readonly string workingDirectory;
        private readonly bool verbose;

        public SplitOperation(string inputFile, bool verbose = false)
        {
            this.inputFile = Path.GetFileName(inputFile);
            this.workingDirectory = Path.GetDirectoryName(inputFile);
            this.verbose = verbose;
        }

        public void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var serializedOutput = MediaHelper.GetMediaInfo(inputFile);
            var chapters = serializedOutput.Chapters;
            var formatDescriptions = serializedOutput.Format;

            var track = count;

            foreach (var chapter in chapters)
            {
                track++;
                this.ProcessChapter(track, chapter, formatDescriptions);
            }
            count = track;
        }

        private void ProcessChapter(int track, Chapter chapter, Format formatDescriptions)
        {
            string filePath = $"{this.inputFile}";
            var fileExtension = Path.GetExtension(filePath);

            var folder = MediaHelper.GetFolder(formatDescriptions, this.workingDirectory);
            var cover = MediaHelper.GetCover(GetFullFilePath(), $"{folder}");

            var target = Path.Combine(folder, $"{track}_{chapter.Tags.Title}{fileExtension}".ToFilePathSafeString());

            // remove existing file
            if (File.Exists(target))
            {
                File.Delete(target);
            }

            var start = Convert.ToDouble(chapter.StartTime, new CultureInfo("en-US"));
            var duration = Convert.ToDouble(chapter.EndTime, new CultureInfo("en-US"));

            var cliParam = new CommandLineParameter(this.GetFullFilePath(), cover, target);
            cliParam.SetStart(start);

            if (!this.verbose)
            {
                cliParam.DisableVerbosityOutput();
            }
            cliParam.SetDuration(duration);
            if (fileExtension == ".m4b" || fileExtension == ".m4a")
            {
                cliParam.AddMp4FastStart();
            }
            cliParam.AddCoverArt();
            cliParam.AddArtist(formatDescriptions.Tags.Artist);
            cliParam.AddChapterTitle(chapter.Tags.Title);
            cliParam.AddTrackNumber(track);

            Console.WriteLine($"Extracting {track}: '{track:D3} - {chapter.Tags.Title}' from '{formatDescriptions.Tags.Album}'");

            var pExtract = new Process
            {
                StartInfo =
                            {
                                UseShellExecute = false,
                                WorkingDirectory = this.workingDirectory,
                                FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe",
                                Arguments = cliParam.GetCliArguments()
                            }
            };
            pExtract.Start();
            // wait for end of extraction.
            pExtract.WaitForExit();
            pExtract.Dispose();
        }

        private string GetFullFilePath()
        {
            return $"{this.workingDirectory}\\{this.inputFile}";
        }
    }
}