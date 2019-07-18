#!/bin/bash

# This script makes sure the most up to date versions of .Net Framework files are in the DotNetEnablement folder, 
# to be used for .Net conversion.

# To run this script on a Windows machine:
#	your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh your-root-project-path
# To run this script on a Mac:
#   First (one time only per machine) run this: 
#       chmod u+x your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh
#   Then to run the script each time:
#	    your-root-project-path/UpdateDotNetFrameworkProjectFiles.sh your-root-project-path

# Arguments are as follows:
#	$1 is used to set path to root of project 
#		eg C:/Reconciliate

project_path=$1
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

# Reconciliate.sln
echo "Solution: create backup of Reconciliate.sln"
cp ./Reconciliate.sln "$backup_folder" 