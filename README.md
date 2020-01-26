# README

## About

AudioChapterSplit is a command line program to split audiobook files. It's based on ffmpeg.

Input can be M4A or M4B files as well as MP3s. The tool will parse the chapter marks
and generate new output file for each chapter within the orginal file. The output will
not be encoded again.

This project is based on [AudioBookSplitter](https://github.com/PrometeoConseil/AudioBookSplitter)

## ffmpeg

It's a bit of hard work to get a decent copy of ffmpeg on Windows. I recommend
to use [media-autobuild_suite](https://github.com/m-ab-s/media-autobuild_suite). 
Compilation will take about an hour. If you run into problems try to disable AOM.

Please note: you need to have ffmpeg.exe and ffprobe.exe in your path!

## Usage

Run `AudioChapterSplit.exe -i "file 1":"file 2":"file n"` to split up the files. 
The new files will be created in a sub folder as mentioned in the album tag.
Track numbers will be set continously over all input files. Additionally, a 
file named "cover.png" will be created.

Run `AudioBookSplitter.exe -i "file 1-n" -v` to see the ffmpeg output.
