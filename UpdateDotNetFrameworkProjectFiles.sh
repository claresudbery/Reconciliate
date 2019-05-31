#!/bin/bash

# This script makes sure the most up to date versions of .Net Framework files are in the DotNetEnablement folder, 
# to be used for .Net conversion.

# To run this script on my Windows machine:
#	your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh
# To run this script on my Macbook:
#   First (one time only per machine) run this: 
#       chmod u+x your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh
#   Then to run the script each time:
#	    your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh mac

# Arguments are as follows:
#	$1 is used to determine path to root of project 
#		"mac" is Mac: your-root-project-path
#		anything else, or unpopulated, is Windows: your-root-project-path

echo "determine project path"
echo "Not storing paths in this script any more - needs fixing - see project_path variable in script - can just use same method of passing path in used in DotNetConversion.sh"
project_path="XXXXXXX"
default_os="win"
# ${1:-default_os} means that if the first argument ($1) is not populated, use $default_os instead
if [ ${1:-default_os} = "mac" ]
then
	project_path="XXXXXXX"
fi
echo "project path is $project_path"

echo "get to the correct location"
cd $project_path

backup_folder="./DotNetCoreEnablement/DotNetFrameworkFiles"
echo "files will be copied to $backup_folder"

# Console project
echo "Console: create backup of proj file"
cp ./Console/Console.csproj "$backup_folder" 

# ConsoleCatchallTests project
echo "ConsoleCatchallTests: create backup of proj file"
cp ./ConsoleCatchallTests/ConsoleCatchallTests.csproj "$backup_folder" 

# ExcelLibrary project
echo "ExcelLibrary: create backup of proj file"
cp ./ExcelLibrary/ExcelLibrary.csproj "$backup_folder" 

# ExcelIntegrationTests project
echo "ExcelIntegrationTests: create backup of proj file"
cp ./ExcelIntegrationTests/ExcelIntegrationTests.csproj "$backup_folder" 

# Interfaces project
echo "Interfaces: create backup of proj file"
cp ./Interfaces/Interfaces.csproj "$backup_folder" 

# SpreadsheetRepoFactoryFactory.cs
echo "SpreadsheetRepoFactoryFactory: create backup of SpreadsheetRepoFactoryFactory.cs"
cp ./Console/Reconciliation/Spreadsheets/SpreadsheetRepoFactoryFactory.cs "$backup_folder" 

# ConsoleCatchall.sln
echo "Solution: create backup of ConsoleCatchall.sln"
cp ./ConsoleCatchall.sln "$backup_folder" 