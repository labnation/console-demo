#!/bin/bash
set -e

if [[ -z "$1" ]]; then
  echo Usage:
  echo 
  echo "$0 <Platform>"
  echo 
  echo "Where <Platform> can be Linux|MacOS|Windows"
  exit -1
fi

HOST=`uname`
if [[ x$HOST != xMINGW* ]]; then
	CMD_PREFIX="mono ./"
else
	CMD_PREFIX="./"
fi
echo Current directory: `pwd`

git submodule init
git submodule update 
${CMD_PREFIX}Protobuild.exe --generate $1 
sed -i~ -e "s/<ProjectTypeGuids>.*<\/ProjectTypeGuids>//" Conscople/Conscople.$1.csproj
sed -i~ -e "s/\(<UseSGen>False<\/UseSGen>\)/\1<Externalconsole>true<\/Externalconsole>/" Conscople/Conscople.$1.csproj

${CMD_PREFIX}.nuget/NuGet.exe restore Conscople.$1.sln

cd mono-curses
./configure
make
cd -
