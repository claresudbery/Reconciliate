## Intro

Note that if you're here because you read [the article on Martin Fowler's site](https://martinfowler.com/articles/class-too-large.html), you probably want to focus on the `Refactor-examples` branch rather than the master branch: `git checkout Refactor-examples`

## Config
There are TWO config files. The first is the main entry point and only contains one line, which is the path to the full config. This is so that you can put your main config in a secure non-public location. If you're Clare and you've forgotten where your config is, check C:/Config/Config.Xml for the location, but also remember that it's stored in the separate ReconciliationConfig private git repo.

The entry-level config is assumed to be in a file called Config.Xml. Its location can be passed in as a command-line argument, but it has a default value which is C:/Config. That won't work unless you're on Windows, so if you're on Mac you HAVE to pass in your entry-level config file location.

There are two config files in the code base: at the root level is Config.xml, and there is also spreadsheet-samples/SampleConfig.xml. SampleConfig.xml contains a starter version of the full config, designed to get you up and running asap out of the box. The simplest way to get started is as follows:

1. Check the various paths in SampleConfig.xml. Change them if necessary (for instance, at the time of writing in SampleConfig.xml, Test_backup_file_path is set to C:/Temp/ManualTesting/TestSpreadsheetBackups. You either need to create these folders or edit that path value in the xml).

On a Mac:  

2. Edit Config.xml so that it contains the path to the repo version of SampleConfig.xml.
3. Pass in the location of Config.Xml when you run the software.

On Windows:  

2. Copy Config.Xml to C:/Config/Config.Xml.
3. Edit Config.xml so that it contains the path to the repo version of SampleConfig.xml.

## Reconciliation:   
This code has been designed to speed up my slightly idiosyncratic accounting process - I can't guarantee it'll be any use to anyone else!  
Follow the instructions on screen.  
For full instructions on how to use the software, see ReconciliationProcess.txt.  
I've made the code base public so that I can talk about coding principles used in here, but you might notice that the full commit history is not present. This is because the original repo was private, so the first year or so of commits are in a separate private repo.  

NB:  
	Expenses:  
	When doing real accounting: Just hit ignore for all the Expenses matches for now, because it's not fully implemented yet.  

## Manual Testing
See [manual-testing.md](manual-testing.md)

## .Net Core   

If you want to run the code in .Net Core:   
	1.	update the stored .Net Framework files, using the script UpdateDotNetFrameworkProjectFiles.sh (see instructions in script)   
	2.	convert to .Net Core using the script DotNetConversion.sh 
	  a. If you're Clare, check the separate private ReconciliationConfig repo - you've stored all the commands for Mac and Windows there  
	  b. There are instructions on using the script in the script itself  
	  c. To get more info on converting to/from .Net Core, see comments in DotNetConversion.sh    
	3a. Make sure all your csv files are in place and your config is correctly set up - see ReconciliationProcess.txt.  
	3b. Be aware that in .Net Core, no actual spreadsheet read/writes occur. Instead a stub with fake data is used (see FakeSpreadsheetRepo.cs).  
	This data is based on the data in the shipped version of Your-Spreadsheet.xlsx. If you want different data, you'll have to edit the code in FakeSpreadsheetRepo.cs.  

	4.	On the command line...  	
	4a) In Windows: Use "dotnet run" from the Console folder or "dotnet test" from the ConsoleCatchallTests folder.  
	4b) On a Mac: Use "dotnet run [path-to-main-config]" from the Console folder or "dotnet test" from the ConsoleCatchallTests folder.  
	5.	If editing code on a Mac, use Visual Studio Code with a test runner installed - see "Write and run some tests" in this post: https://insimpleterms.blog/2018/10/31/adding-nunit-tests-to-a-net-core-console-app/   
	6. ?? Store the latest .Net Core files which will have been created during this process, using the script UpdateDotNetCoreProjectFiles.sh (see instructions in script) (I'm not sure about this - I added this in May 2019 because otherwise I'm not sure why UpdateDotNetCoreProjectFiles.sh even exists)  
	7. Use DotNetConversion.sh again to convert back to .Net Framework  
	8. Commit new versions of DotNetFramework project files  
Gotcha: Errors about "dll already in use":   
	Check you don't have reconciliation software open already via taskbar shortcut!   
	Check out previous version of code, rebuild, go back to current version, rebuild again  
	Delete relevant bin folders  
	Switch from Debug to Release, rebuild, go back to Debug, rebuild again  
	Close and reopen VS  
Gotcha when switching back from .Net Core to .Net Framework: "dll / exe could not be found"	
	Delete bin and obj folders for projects that are in both .Net Core and .Net Framework
	Close and reopen VS
	Rebuild all
	
## Troubleshooting
### HRESULT exception In Windows when writing to spreadsheet
Go to task manager, scroll down to processes below the line, and get rid of ALL instances of Excel you find (right-click | End task).

## Code flow diagram:

There is a code chart available (created June 2020) that shows the most important code elements and what order they happen in, so you can see where best to insert new functionality:  
(I think it also helps to show where the code still needs a fair bit of work to make simpler / more accessible).  
Note that it is based on the master branch, not the `Refactor-examples` branch, so there are some minor differences between the two:    

![code chart](/Reconciliate-flow-chart.jpg)
	

An example of a C# app with a Windows interface that edits an Excel spreadsheet:  
https://www.codeproject.com/Tips/696864/Working-with-Excel-Using-Csharp    
    
