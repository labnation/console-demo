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
cd DeviceInterface
git submodule init
git submodule update
cd -

${CMD_PREFIX}Protobuild.exe --generate $1 
sed -i~ -e "s/<ProjectTypeGuids>.*<\/ProjectTypeGuids>//" SmartScopeConsole/SmartScopeConsole.$1.csproj
sed -i~ -e "s/\(<UseSGen>False<\/UseSGen>\)/\1<Externalconsole>true<\/Externalconsole>/" SmartScopeConsole/SmartScopeConsole.$1.csproj

${CMD_PREFIX}.nuget/NuGet.exe restore SmartScopeConsole.$1.sln
