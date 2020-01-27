using CommandLine;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine.Text;

namespace AudioChapterSplit
{
    internal class Program
    {

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
                throw new Exception();
            }

            var parsedResult = ((Parsed<Options>)result).Value;

            try
            {
                if (parsedResult.Merge)
                {
                    // we want to merge the files
                }
                else
                {
                    // we want to split the file by chapters
                    foreach (string inputFile in parsedResult.SourceFiles)
                    {
                        new Split(inputFile, parsedResult.Debug).Encode();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                parser?.Dispose();
            }
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