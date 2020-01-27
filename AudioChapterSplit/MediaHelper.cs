using AudioChapterSplit.ffprobeOutput;
using System;
using System.Diagnostics;
using System.IO;

namespace AudioChapterSplit
{
    class MediaHelper
    {

        private static void ExtractCoverArt(string inputFile, string outputPath)
        {
            var p = new Process
            {
                StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = ".\\ffmpeg\\ffmpeg.exe",
                            Arguments = $" -i \"{inputFile}\" -hide_banner -loglevel panic -c copy -map 0:v -map -0:V -vframes 1 \"{outputPath}\" "
                        }
            };

            p.Start();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Dispose();
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
                folder += Path.Combine(path, $"{formatDescriptions.Tags.Album.ToFilePathSafeString()}");
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
