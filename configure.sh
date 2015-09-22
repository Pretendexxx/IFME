#!/bin/sh
cd "$(dirname $(readlink -f $0))"

echo "Creating folder"
mkdir "prerequisite/linux"
mkdir "prerequisite/linux/64bit"
mkdir "prerequisite/linux/64bit/plugins"
mkdir "prerequisite/allos/"
mkdir "prerequisite/allos/extension"

echo "Downloading References"
wget --no-check-certificate https://github.com/Anime4000/IFME/releases/download/v5.0-beta.8/INIFileParser.dll -O "references/INIFileParser.dll"
wget --no-check-certificate https://github.com/x265/MediaInfoDotNet/releases/download/v0.7.8/MediaInfoDotNet.dll -O "references/MediaInfoDotNet.dll"

echo "Downloading extension"
wget --no-check-certificate https://github.com/x265/HFRGen/releases/download/v0.2/hfrgen.dll -O "prerequisite/allos/extension/hfrgen.dll"
wget --no-check-certificate https://github.com/x265/HoloBenchmark/releases/download/v0.0.2/holobenchmark.dll -O "prerequisite/allos/extension/holobenchmark.dll"
wget --no-check-certificate https://github.com/x265/Nemupad/releases/download/0.0.3.1/nemupad.dll -O "prerequisite/allos/extension/nemupad.dll"

echo "Downloading plugins"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/faac.ifx -O "prerequisite/linux/64bit/plugins/faac.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/ffmpeg.ifx -O "prerequisite/linux/64bit/plugins/ffmpeg.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/ffmsindex.ifx -O "prerequisite/linux/64bit/plugins/ffmsindex.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/mkvtool.ifx -O "prerequisite/linux/64bit/plugins/mkvtool.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/mp4box.ifx -O "prerequisite/linux/64bit/plugins/mp4box.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/mp4fpsmod.ifx -O "prerequisite/linux/64bit/plugins/mp4fpsmod.ifx"
wget http://sourceforge.net/projects/ifme/files/plugins/linux/64bit/x265gcc.ifx -O "prerequisite/linux/64bit/plugins/x265gcc.ifx"

echo "Unpacking plugins"
7za x "prerequisite/linux/64bit/plugins/faac.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/ffmpeg.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/fmpeg-ogg.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/ffmsindex.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/mkvtool.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/mp4box.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/mp4fpsmod.ifx" -y -o"prerequisite/linux/64bit/plugins/"
7za x "prerequisite/linux/64bit/plugins/x265gcc.ifx" -y -o"prerequisite/linux/64bit/plugins/"

echo "Delete cache"
find . -name "*.ifx" -type f -delete

echo "Done!!!"
sleep 3