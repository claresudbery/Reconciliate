#!/bin/bash

# This script will convert the Reconciliate project from .Net to .Net Framework or vice versa.

# To run this script on a Windows machine:
#	Either: Convert to .Net Core:
#		your-root-project-path/DotNetConversion.sh tocore your-root-project-path
#	Or: Convert to .Net Framework:
#		your-root-project-path/DotNetConversion.sh tofram your-root-project-path
# To run this script on a Macbook:
#   First (one time only per machine) run this: 
#       chmod u+x your-root-project-path/DotNetConversion.sh
#			(replace path with your own)
#   Then to run the script each time:
#	    Either: Convert to .Net Core:
#           your-root-project-path/DotNetConversion.sh tocore your-root-project-path
#	    Or: Convert to .Net Framework:
#		    your-root-project-path/DotNetConversion.sh tofram your-root-project-path

# Arguments are as follows:
#	$1 is used to determine whether converting to .Net Core or to .Net Standard
#		"tocore" means convert to a .Net Core project
#		anything else will be .Net Framework
#	$2 is used to set path to root of project 
#		eg C:/Reconciliate

project_path=$2
echo "project path is $project_path"

echo "get to the correct location"
cd $project_path

echo "determine whether we're converting to .Net Core or .Net Framework"
source_folder="./DotNetCoreEnablement/DotNetFrameworkFiles"
dest_folder="./DotNetCoreEnablement/DotNetCoreFiles/Backups"
default_action="tostan"
# ${1:-default_action} means that if the first argument ($1) is not populated, use $default_action instead
if [ ${1:-default_action} = "tocore" ]
then
	echo "We are converting from .Net Framework to .Net Core"
	source_folder="./DotNetCoreEnablement/DotNetCoreFiles"
	dest_folder="./DotNetCoreEnablement/DotNetFrameworkFiles/Backups"
else
	echo "We are converting from .Net Core to .Net Framework"
fi
echo "new files will be taken from $source_folder"
echo "old files will be backed up to $dest_folder"

echo "Deleting bin and obj folders"
rm -r ./Console/bin
rm -r ./Console/obj
rm -r ./ConsoleCatchallTests/bin
rm -r ./ConsoleCatchallTests/obj
rm -r ./ExcelLibrary/bin
rm -r ./ExcelLibrary/obj
rm -r ./ExcelIntegrationTests/bin
rm -r ./ExcelIntegrationTests/obj
rm -r ./Interfaces/bin
rm -r ./Interfaces/obj

# Console project
echo "Console: create backup of proj file and replace with new"
mv ./Console/Console.csproj "$dest_folder/Console.csproj.$(date +%F_%H:%M:%S)"
cp "$source_folder/Console.csproj" ./Console

# ConsoleCatchallTests project
echo "ConsoleCatchallTests: create backup of proj file and replace with new"
mv ./ConsoleCatchallTests/ConsoleCatchallTests.csproj "$dest_folder/ConsoleCatchallTests.csproj.$(date +%F_%H:%M:%S)"
cp "$source_folder/ConsoleCatchallTests.csproj" ./ConsoleCatchallTests

# ExcelLibrary project
echo "ExcelLibrary: create backup of proj file and replace with new"
mv ./ExcelLibrary/ExcelLibrary.csproj "$dest_folder/ExcelLibrary.csproj.$(date +%F_%H:%M:%S)"
cp "$source_folder/ExcelLibrary.csproj" ./ExcelLibrary

# ExcelIntegrationTests project
echo "ExcelIntegrationTests: create backup of proj file and replace with new"
mv ./ExcelIntegrationTests/ExcelIntegrationTests.csproj "$dest_folder/ExcelIntegrationTests.csproj.$(date +%F_%H:%M:%S)"
cp "$source_folder/ExcelIntegrationTests.csproj" ./ExcelIntegrationTests

# Interfaces project
echo "Interfaces: create backup of proj file and replace with new"
mv ./Interfaces/Interfaces.csproj "$dest_folder/Interfaces.csproj.$(date +%F_%H:%M:%S)"
cp "$source_folder/Interfaces.csproj" ./Interfaces

# SpreadsheetRepoFactoryFactory.cs
echo "SpreadsheetRepoFactoryFactory: create backup of SpreadsheetRepoFactoryFactory.cs and replace with new"
mv ./Console/Reconciliation/Spreadsheets/SpreadsheetRepoFactoryFactory.cs "$dest_folder/SpreadsheetRepoFactoryFactory.cs.$(date +%F_%H:%M:%S)"
cp "$source_folder/SpreadsheetRepoFactoryFactory.cs" ./Console/Spreadsheets

# ConsoleCatchall.sln
echo "Solution: create backup of ConsoleCatchall.sln and replace with new"
mv ./ConsoleCatchall.sln "$dest_folder/ConsoleCatchall.sln.$(date +%F_%H:%M:%S)"
cp "$source_folder/ConsoleCatchall.sln" ./

# ${1:-default_action} means that if the first argument ($1) is not populated, use $default_action instead
if [ ${1:-default_action} = "tocore" ]
then
	echo "Building ExcelLibrary and ExcelIntegrationTests, to avoid errors in Rider on the Mac about missing dlls"
	cd ExcelLibrary
	dotnet build
	cd ..
	cd ExcelIntegrationTests
	dotnet build
fi
