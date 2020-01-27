# README

## About

AudioChapterSplit is a command line program to split audiobook files. It's based on ffmpeg.

Input can be M4A or M4B files as well as MP3s. The tool will parse the chapter marks
and generate new output file for each chapter within the orginal file. The output will
not be reencoded again.

This project is based on [AudioBookSplitter](https://github.com/PrometeoConseil/AudioBookSplitter).

## ffmpeg

The releases come prebundled with ffmpeg. If you want your own ffmpeg version and 
not the bundled one: it's not easy to get a decent copy of ffmpeg on Windows. I 
recommend to use [media-autobuild_suite](https://github.com/m-ab-s/media-autobuild_suite). 
Compilation will take about an hour. If you run into problems try to disable AOM. 

## Usage

Run `AudioChapterSplit.exe -i "file 1" "file 2" "file n"` to split up the files. 
The new files will be created in a sub folder as mentioned in the album tag.
Track numbers will be set continously over all input files. Additionally, a 
file named "cover.png" will be created, cover art will be embedded if possible.

Run `AudioBookSplitter.exe -i "file 1-n" -v` to see the ffmpeg output.
