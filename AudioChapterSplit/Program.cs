using CommandLine;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine.Text;
using AudioChapterSplit.ffprobeOutput;

namespace AudioChapterSplit
{
    internal class Program
    {

        private static int count = 0;

        private static void Main(string[] args)
        {
            var parser = new Parser(config => config.HelpWriter = null);
            var result = parser.ParseArguments<Options>(args);
            result.WithNotParsed(errors =>
            {
                foreach (var error in errors)
                {
                    if (error.Tag != ErrorType.HelpRequestedError &&
                        error.Tag != ErrorType.VersionRequestedError) continue;

                    Console.WriteLine(BuildHelp(result));
                    Environment.Exit(0);
                }

                var myHelpText = HelpText.AutoBuild(result, onError => BuildHelp(result), onExample => onExample);
                Console.Error.WriteLine(myHelpText);
                Environment.Exit(1);
            });

            if (result.GetType() != typeof(Parsed<Options>))
            {
                return;
            }

            var parsedResult = ((Parsed<Options>)result).Value;

            try
            {
                foreach (string inputFile in parsedResult.SourceFiles)
                {
                    Encode(inputFile, parsedResult.Verbose);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void ExtractCoverArt(string inputFile, string outputPath)
        {
            var p = new Process
            {
                StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = "ffmpeg.exe",
                            Arguments = $" -i \"{inputFile}\" -hide_banner -loglevel panic -c copy -map 0:v -map -0:V -vframes 1 \"{outputPath}\\cover.png\" "
                        }
            };

            p.Start();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }

        private static void Encode(string source, bool verbose = false)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (!File.Exists(source))
            {
                Console.WriteLine($"File '{source}' not found.");
            }
            else
            {
                // using ffprobe, extract chapter list and info (album, artist, date, ...) from mp3.
                // Command is : ffprobe -v quiet -print_format json -show_chapters -show_format <filename>.mp3

                // Redirect the output stream of the child process.
                var p = new Process
                {
                    StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = "ffprobe.exe",
                            Arguments = $" -hide_banner -loglevel panic -print_format json -show_chapters -show_format \"{source}\""
                        }
                };

                // Start the child process.
                p.Start();

                // Read the output stream first and then wait.
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                var serializedOutput = JsonConvert.DeserializeObject<ffprobeOutput.RootObject>(output);
                var chapters = serializedOutput.Chapters;
                var formatDescriptions = serializedOutput.Format;

                string filePath = $"{source}";
                var fileExtension = Path.GetExtension(filePath);

                var folder = GetFolder(formatDescriptions);
                var cover = GetCover(source, folder);
                var track = count;

                foreach (var chapter in chapters)
                {
                    track++;
                    Console.WriteLine($"Trying to create \"{track:D2} - {chapter.Tags.Title}.mp3\" into folder \"{folder}\"");
                    var target = Path.Combine(folder, $"{track}_{chapter.Tags.Title}{fileExtension}".ToFilePathSafeString());

                    // remove existing file
                    if (File.Exists(target)) {
                        File.Delete(target);
                    }

                    var start = Convert.ToDouble(chapter.StartTime, new CultureInfo("en-US"));
                    var duration = Convert.ToDouble(chapter.EndTime, new CultureInfo("en-US")) - start;

                    var arguments = $"-i \"{source}\" -i \"{cover}\" -stats -ss {start.ToString(new CultureInfo("en-US"))} -threads 0 ";
                    if (!verbose) {
                        arguments += $" -hide_banner -loglevel panic ";
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
                    
                    Console.WriteLine($"Extracting {track}/{chapters.Length}: '{track:D2} - {chapter.Tags.Title}' from '{formatDescriptions.Tags.Album}'");

                    var pExtract = new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = false,
                                FileName = "ffmpeg.exe",
                                Arguments = arguments
                            }
                    };
                    pExtract.Start();
                    // wait for end of extraction.                    
                    pExtract.WaitForExit();
                }
                count = track;
            }
        }

        private static string GetCover(string source, string folder)
        {
            var cover = Path.Combine(folder, @"cover.png".ToFilePathSafeString());
            if (File.Exists(cover))
            {
                File.Delete(cover);
            }

            ExtractCoverArt(source, folder);

            return cover;
        }

        private static string GetFolder(Format formatDescriptions)
        {
            var folder = string.Empty;

            try
            {
                folder += $".\\{formatDescriptions.Tags.Album.ToFilePathSafeString()}";
            }
            catch (Exception)
            {
                Console.WriteLine($"[WARN] FAILED to format album  '{formatDescriptions.Tags.Album}' as a valid file path part.");
            }

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        private static HelpText BuildHelp(ParserResult<Options> result)
        {
            var assembly = typeof(Program).GetTypeInfo().Assembly;

            var assemblyTitleAttribute = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute)).SingleOrDefault() as AssemblyTitleAttribute;
            var assemblyCopyrightAttribute = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute)).SingleOrDefault() as AssemblyCopyrightAttribute;
            var assemblyCompanyAttribute = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute)).SingleOrDefault() as AssemblyCompanyAttribute;
            var version = assembly.GetName().Version.ToString().ToString(CultureInfo.InvariantCulture);

            var nHelpText = new HelpText(SentenceBuilder.Create(), $"{assemblyTitleAttribute?.Title} {version}"
                                                                   + $"{(assemblyCopyrightAttribute == null && assemblyCompanyAttribute == null ? "" : "\r\n" + (assemblyCopyrightAttribute?.Copyright))} {assemblyCompanyAttribute?.Company}"
                                                                   + $"{((!(assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute)).SingleOrDefault() is AssemblyDescriptionAttribute assemblyDescriptionAttribute)) ? "" : "\r\n" + assemblyDescriptionAttribute.Description)}")
            {
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true,
                MaximumDisplayWidth = 4000,
                AddEnumValuesToHelpText = true
            };
            nHelpText.AddOptions(result);
            return HelpText.DefaultParsingErrorsHandler(result, nHelpText);
        }

    }
}