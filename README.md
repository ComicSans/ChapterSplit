# README

## About ChapterSplit

ChapterSplit is a command line program to split and merge audiobook files. It's based on ffmpeg.

## Usage

### Splitting big files

Input can be M4A or M4B files as well as MP3s. Those files should have chapter marks as
well as album and artist information.

The tool will parse the chapter marks and generate new output files for each chapter within
the orginal file. If no chapters were found each input file will be handled as a chapter.

Please note: the output will not be reencoded again.

Run `AudioChapterSplit.exe -i "file"` to split up the file. You can pass more than one
file like this: `AudioChapterSplit.exe -i "file1" "file 2" "file n"`.

The new files will be created in a sub folder "Artist - Album".

Track numbers will be set continously over all input files. Additionally, a file named 
"cover.png" will be created. Cover art will be embedded in the splitted files if possible.

### Merging smaller files into one single audiobook file

Please note: the output will always be m4b and the files will always be reencoded again.

Relevant metadata information (artist, album and genre) for the output file will be taken from 
the first input file.

Run `AudioChapterSplit.exe -i "file 1" "file 2" "file n" -m` to split up the file. You can pass
as many files as you like.

## ffmpeg

Use the optional parameter "-d" to see the ffmpeg output.

The releases come prebundled with ffmpeg. If you want your own ffmpeg version and 
not the bundled one: it's not easy to get a decent copy of ffmpeg with enabled libfdk_aac on 
Windows. I recommend to use [media-autobuild_suite](https://github.com/m-ab-s/media-autobuild_suite). 
Compilation will take a whil. If you run into problems try disabling AOM. 
