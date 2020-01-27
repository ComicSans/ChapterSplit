using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace AudioChapterSplit
{
    class Split
    {
        private static int count = 0;
        readonly string inputFile;
        readonly string workingDirectory;
        readonly bool verbose;

        public Split(string inputFile, bool verbose)
        {
            this.inputFile = Path.GetFileName(inputFile);
            this.workingDirectory = Path.GetDirectoryName(inputFile);
            this.verbose = verbose;
        }

        public void Encode()
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (!File.Exists(GetFullFilePath()))
            {
                Console.WriteLine($"File '{GetFullFilePath()}' not found.");
            }
            else
            {
                // using ffprobe, extract chapter list and info (album, artist, date, ...) from file.
                // Command is : ffprobe -print_format json -show_chapters -show_format <filename>

                // Redirect the output stream of the child process.
                var p = new Process
                {
                    StartInfo =
                        {
                            UseShellExecute = false,
                            WorkingDirectory = this.workingDirectory,
                            RedirectStandardOutput = true,
                            FileName = ".\\ffmpeg\\ffprobe.exe",
                            Arguments = $"-hide_banner -loglevel panic -print_format json -show_chapters -show_format \"{this.inputFile}\""
                        }
                };

                // Start the child process.
                p.Start();

                // Read the output stream first and then wait.
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Dispose();

                var serializedOutput = JsonConvert.DeserializeObject<ffprobeOutput.RootObject>(output);
                var chapters = serializedOutput.Chapters;
                var formatDescriptions = serializedOutput.Format;

                string filePath = $"{this.inputFile}";
                var fileExtension = Path.GetExtension(filePath);

                var folder = MediaHelper.GetFolder(formatDescriptions, this.workingDirectory);
                var cover = MediaHelper.GetCover(GetFullFilePath(), $"{folder}");

                var track = count;

                foreach (var chapter in chapters)
                {
                    track++;
                    var target = Path.Combine(folder, $"{track}_{chapter.Tags.Title}{fileExtension}".ToFilePathSafeString());

                    // remove existing file
                    if (File.Exists(target))
                    {
                        File.Delete(target);
                    }

                    var start = Convert.ToDouble(chapter.StartTime, new CultureInfo("en-US"));
                    var duration = Convert.ToDouble(chapter.EndTime, new CultureInfo("en-US")) - start;

                    var arguments = $"-i \"{this.GetFullFilePath()}\" -i \"{cover}\" -stats -ss {start.ToString(new CultureInfo("en-US"))} -threads 0 ";
                    if (!this.verbose)
                    {
                        arguments += $"-hide_banner -loglevel panic ";
                    }
                    arguments += $"-t {duration.ToString(new CultureInfo("en-US"))} ";
                    // copy stream
                    arguments += "-c copy -map_chapters -1 ";
                    // add m4b fast start flag
                    if (fileExtension == ".m4b" || fileExtension == ".m4a")
                    {
                        arguments += $"-movflags +faststart -f mp4 ";
                    }
                    // add cover art
                    arguments += $"-map 0:0 -map 1:0 -id3v2_version 3 -metadata:s:v title=\"Album cover\" -metadata:s:v comment=\"Cover(front)\" ";
                    // add artist as tag
                    arguments += $"-metadata artist=\"{formatDescriptions.Tags.Artist}\" ";
                    // add chapter title as title tag
                    arguments += $"-metadata title=\"{chapter.Tags.Title}\" ";
                    // add track number title as tag
                    arguments += $"-metadata track={track} \"{target}\"";

                    Console.WriteLine($"Extracting {track}: '{track:D2} - {chapter.Tags.Title}' from '{formatDescriptions.Tags.Album}'");

                    var pExtract = new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = false,
                                WorkingDirectory = this.workingDirectory,
                                FileName = ".\\ffmpeg\\ffmpeg.exe",
                                Arguments = arguments
                            }
                    };
                    pExtract.Start();
                    // wait for end of extraction.                    
                    pExtract.WaitForExit();
                    pExtract.Dispose();
                }
                count = track;
            }
        }

        private string GetFullFilePath()
        {
            return $"{this.workingDirectory}\\{this.inputFile}";
        }
    }
}
