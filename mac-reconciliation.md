## Full Reconciliation on a Mac
(Note that several commit Ids are referenced here - just in case those commits are removed by a rebase, they are all from the series of about 10 commits that start with the one with description "Show user what balance data is being written to Totals worksheet.", committed 12/12/19 to master branch).
At the time of writing, the way to achieve this is as follows:
* Depending on your data, you'll probably want to edit your SampleConfig.xml to contain real data
    * (If you're Clare, see below).
* Edit the files in the folder Console/Reconciliation/Spreadsheets/FakeSpreadsheetData
    * These files are used to replace the data that would otherwise be read from a spreadsheet.
    * Change the data to match the data in your actual spreadsheet.
    * To get a feel for how these values relate to an actual spreadsheet, look at Your-Spreadsheet.xlsx (in reconciliation-samples/For-debugging) - they are designed to match this data.
* Edit the mortgage row so that its date is the month BEFORE the first month you want to budget for.
    * This is the row in FakeRows.cs under MainSheetNames.Bank_out that has the description FakeSpreadsheetRepo.FakeMortgageDescription
    * It's the first date on the row that counts, not the second one.
    * See commit 6054e6e for an example.
    * !! Remember to reverse the change afterwards! See commit 3e07895.
* At the time of writing a small hack is also needed:
    * Edit the function ExpectedIncomeFile.Finish so that it writes to an ExpectedIn csv file.
    * See commits 00a6815 and aedce66 for an example.
    * If you do it this way, you might need to create the csv file first (not sure about this).
    * !! Remember to undo the change once you're done! See commit b5dc4e6.
* All the data that is normally written to the spreadsheet will instead be written to files with the suffix "-recon"
    * They will be in the same place as your source csvs (the default is reconciliation-samples/For-debugging)
* Note that some data is normally written to the Totals tab on the spreadsheet.
    * This will not be placed in a -recon csv. Instead this is output to the command line.
    * You'll see this when reconciling credit cards - just after you enter DD amounts at the start of reconciliation.
    * It looks like this: "Updating Totals with CredCard1 Balance 1000 '!! CredCard1 bal recorded from statement dated May 2019'."

### If you're Clare
* In the private ReconciliationConfig code base, there is real config set up under ForMac/RealConfig.xml
    * You'll also need to edit Config.xml in the same location, so that it points to RealConfig.xml.
    * For commands to run from the command line, see script-commands.txt in the same location.
* In the same code base, there are real csv files in RealCsvs
* In the same code base, there are examples of the changes I made to the code base to get it working for BankIn.
    * See the RealFakeData sub-folder.