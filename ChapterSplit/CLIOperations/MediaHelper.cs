using ChapterSplit.ffprobeOutput;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace ChapterSplit
{
    internal class MediaHelper
    {
        private static void ExtractCoverArt(string inputFile, string outputPath)
        {
            var p = new Process
            {
                StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe",
                            Arguments = $" -i \"{inputFile}\" -hide_banner -loglevel panic -c copy -map 0:v -map -0:V -vframes 1 \"{outputPath}\" "
                        }
            };

            p.Start();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Dispose();
        }

        public static RootObject GetMediaInfo(string inputFile)
        {
            // using ffprobe, extract chapter list and info (album, artist, date, ...) from file.
            // Command is : ffprobe -print_format json -show_chapters -show_format <filename>

            // Redirect the output stream of the child process.

            var p = new Process
            {
                StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffprobe.exe",
                            Arguments = $"-hide_banner -loglevel panic -print_format json -show_chapters -show_format \"{inputFile}\""
                        }
            };

            // Start the child process.
            p.Start();

            // Read the output stream first and then wait.
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Dispose();

            return JsonConvert.DeserializeObject<ffprobeOutput.RootObject>(output);
        }

        public static string GetCover(string source, string folder)
        {
            var cover = Path.Combine(folder, @"cover.png".ToFilePathSafeString());
            if (File.Exists(cover))
            {
                File.Delete(cover);
            }

            ExtractCoverArt(source, cover);

            return cover;
        }

        public static string GetFolder(Format formatDescriptions, string path)
        {
            var folder = string.Empty;

            try
            {
                folder += Path.Combine(path, $"{formatDescriptions.Tags.Artist.ToFilePathSafeString()} - {formatDescriptions.Tags.Album.ToFilePathSafeString()}");
            }
            catch (Exception)
            {
                Console.WriteLine($"[WARN] FAILED to format album  '{formatDescriptions.Tags.Album}' as a valid file path part.");
                throw;
            }

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}