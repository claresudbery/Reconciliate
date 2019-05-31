## Reconciliation:   
This code has been designed to speed up my slightly idiosyncratic accounting process - I can't guarantee it'll be any use to anyone else!  
Follow the instructions on screen.  
For full instructions on how to use the software, see ReconciliationProcess.txt.  
I've made the code base public so that I can talk about coding principles used in here, but you might notice that the full commit history is not present. This is because the original repo was private, so the first year or so of commits are in a separate private repo.  

NB:  
	Expenses:  
	When doing real accounting: Just hit ignore for all the Expenses matches for now, because it's not fully implemented yet.  

## .Net Core   

If you want to run the code in .Net Core:   
	1.	update the stored .Net Framework files, using the script UpdateDotNetFrameworkProjectFiles.sh (see instructions in script)   
	2.	convert to .Net Core using the script DotNetConversion.sh (there are instructions on using the script in the script itself)   
		To get more info on converting to/from .Net Core, see comments in DotNetConversion.sh  
	3.	On the command line in Windows, use "dotnet run [path-to-csv-files]" from the Console folder or "dotnet test" from the ConsoleCatchallTests folder. On the command line on a Mac, create a dedicated folder in [path-chosen-by-you] and put some csv files there, then use "dotnet run [path-chosen-by-you]"    
	4.	If editing code on a Mac, use Visual Studio Code with a test runner installed - see "Write and run some tests" in this post: https://insimpleterms.blog/2018/10/31/adding-nunit-tests-to-a-net-core-console-app/   
	5. ?? Store the latest .Net Core files which will have been created during this process, using the script UpdateDotNetCoreProjectFiles.sh (see instructions in script) (I'm not sure about this - I added this in May 2019 because otherwise I'm not sure why UpdateDotNetCoreProjectFiles.sh even exists)  
	6. Use DotNetConversion.sh again to convert back to .Net Framework  
	7. Commit new versions of DotNetFramework project files  
Gotcha: Errors about "dll already in use":   
	Check you don't have reconciliation software open already via taskbar shortcut!   
	Check out previous version of code, rebuild, go back to current version, rebuild again  
	Delete relevant bin folders  
	Switch from Debug to Release, rebuild, go back to Debug, rebuild again  
	Close and reopen VS  
	
An example of a C# app with a Windows interface that edits an Excel spreadsheet:  
https://www.codeproject.com/Tips/696864/Working-with-Excel-Using-Csharp    
    