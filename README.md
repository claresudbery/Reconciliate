## Refactoring Example  
If you see this text, it means you are in the Refactor-genericise-start branch of this code base.  
This branch has been created to demonstrate some refactoring code.  
It is NOT the most up to date version of the code. It will contain bugs.  
Also, it is deliberately in need of refactoring. Because that's the point. :)  
Note: This branch was based off the Refactor-eg-genericise branch, where I deliberately unravelled a lot of code for demonstration purposes.  
(It took flippin' ages. I don't think I'm going to do that again.)  

## Reconciliation:   
This code has been designed to speed up my slightly idiosyncratic accounting process - I can't guarantee it'll be any use to anyone else!  
Follow the instructions on screen.  
For full instructions on how to use the software, see ReconciliationProcess.txt.  
I've made the code base public so that I can talk about coding principles used in here, but you might notice that the full commit history is not present. This is because the original repo was private, so the first year or so of commits are in a separate private repo.  

NB:  
	Expenses:  
	When doing real accounting: Just hit ignore for all the Expenses matches for now, because it's not fully implemented yet.  

## Manual Testing

If you want to do a quick manual end to end test, there are files in /reconciliation-samples/For debugging which you can use.  
They contain simple data which is hopefully easy enough to reason about.  
You need to copy the .txt and .csv files into the folder you have configured as DefaultFilePath in your xml config.  
You can copy the spreadsheet (*.xlsx) into the folder you have configured as SourceDebugSpreadsheetPath in your xml config.  
You need to be sure that all file names match what you have in your config. If you haven't edited your config then everything will work as-is, as long as you have followed the instructions to set up your config in ReconciliationProcess.txt.  
Then you can run the software in one of the debug modes.  
Note that at the time of writing, when running in debug mode the debug spreadsheet is recreated every time you do a different type of reconciliation.  
So, for instance, if you do Bank In then Bank Out, you won’t see Bank In stuff unless you look at the spreadsheet between the two reconciliations.  
	
## .Net Core   

If you want to run the code in .Net Core:   
	1.	update the stored .Net Framework files, using the script UpdateDotNetFrameworkProjectFiles.sh (see instructions in script)   
	2.	convert to .Net Core using the script DotNetConversion.sh (there are instructions on using the script in the script itself)   
		To get more info on converting to/from .Net Core, see comments in DotNetConversion.sh  
	3a. Make sure all your csv files are in place and your config is correctly set up - see ReconciliationProcess.txt.  
	3b. Be aware that in .Net Core, no actual spreadsheet read/writes occur. Instead a stub with fake data is used (see FakeSpreadsheetRepo.cs).  
	This data is based on the data in the shipped version of Your-Spreadsheet.xlsx. If you want different data, you'll have to edit the code.  
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

	
An example of a C# app with a Windows interface that edits an Excel spreadsheet:  
https://www.codeproject.com/Tips/696864/Working-with-Excel-Using-Csharp    
    