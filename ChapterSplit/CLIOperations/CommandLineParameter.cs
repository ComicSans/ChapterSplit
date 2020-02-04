using System;
using System.Globalization;

namespace ChapterSplit.CLIOperations
{
    internal class CommandLineParameter
    {
        private string cli = "";
        private string target = "";
        private string verbose = "";
        private string mp4 = "";
        private string coverArt = "";
        private string artistName = "";
        private string chapterName = "";
        private string trackNumber = "";
        private string duration;
        private string copyStream = "-c copy -map_chapters -1";
        private string otherParams = "-threads 0";
        private string start;

        public CommandLineParameter(string inputFilePath, string coverFilePath, string target)
        {
            this.cli = $"-i \"{inputFilePath}\" -i \"{coverFilePath}\" -y";
            this.target = target;
        }

        public string GetCliArguments()
        {
            return $"{this.cli} {this.start} {this.otherParams} {this.verbose} {this.duration} {this.copyStream} {this.mp4} {this.coverArt} {this.artistName} {this.chapterName} {this.trackNumber} \"{this.target}\"";
        }

        public void SetStart(double start)
        {
            this.start = $"-ss {start.ToString(new CultureInfo("en-US"))}";
        }

        public void AddCoverArt()
        {
            this.coverArt = $"-map 0:0 -map 1:0 -id3v2_version 3 -metadata:s:v title=\"Album cover\" -metadata:s:v comment=\"Cover(front)\"";
        }

        public void AddMp4FastStart()
        {
            this.mp4 = $"-movflags +faststart -f mp4";
        }

        public void AddArtist(String artistName)
        {
            this.artistName = $"-metadata artist=\"{artistName}\"";
        }

        public void SetDuration(double duration)
        {
            this.duration = $"-t {duration.ToString(new CultureInfo("en-US"))}";
        }

        public void DisableVerbosityOutput()
        {
            this.verbose = $"-hide_banner -loglevel panic -nostats";
        }

        public void AddChapterTitle(String chapterName)
        {
            this.chapterName = $"-metadata title=\"{chapterName}\"";
        }

        public void AddTrackNumber(int trackNumber)
        {
            this.trackNumber = $"-metadata track={trackNumber}";
        }
    }
}