# Manual Testing

If you want to do a quick manual end to end test, there are files in `reconciliation-samples/For-debugging` which you can use. They contain simple data which is hopefully easy enough to reason about.

You'll need to be aware of how your config works. See the config section in [README.md](/README.md).

If you're Clare (and you're not planning an out-of-the-box vanilla test), all the config and other files you need are in the private ReconciliationConfig code base, and you only need to do step 1 below. Don't forget, you probably DON'T want to check the changed spreadsheet back into the config repo!

The quickest way to get up and running for testing purposes is:

1. Copy Config.Xml to C:/Config/Config.Xml (or if you're Clare, copy C:/Config/OutOfTheBoxTestConfig.xml into C:/Config/Config.Xml for duplicating out-of-the-box testing, or C:/TestConfig.xml for more realistic test data).
2. Check paths in SampleConfig.xml: At the time of writing, Test_backup_file_path is set to C:/Temp/ManualTesting/TestSpreadsheetBackups. You either need to create these folders or edit that path value in the xml.
3. Edit Config.xml so that it contains the path to the repo version of SampleConfig.xml (which is in the spreadsheet-samples folder).
(If you're on a Mac, it's similar but slightly different - see [README.md](/README.md)).
4. Jump to [the next section](#running-the-tests) and start testing.

If you haven't edited your config at all then everything will work as-is. If you have edited your config, you need to do the following:

Copy some .txt and .csv files from `reconciliation-samples/For-debugging` into the folder you have configured as DefaultFilePath in your xml config. Note that if you have already run some tests, depending on your config you may see a lot of extra csv files in that folder which are generated by the software, are not in source control (they're in `.gitignore`) and are not needed to start the tests off. You only need to copy the following files: 

* ActualBank.csv
* CredCard1.csv
* CredCard1InOutPending.csv
* CredCard2.csv
* Pending.txt

You should also copy the spreadsheet (file with extension xlsx) into the folder you have configured as SourceDebugSpreadsheetPath in your config (but if you're Clare, use the version in the ReconciliationConfig code base - in For debugging\Quick and simple).

After you've copied the csv / txt files you may need to rename them, depending on your config. Pending.txt should not be renamed, but the other four file names should match what you have in the following config elements: 

* Default_bank_file_name
* Default_cred_card1_file_name
* Default_cred_card1_in_out_pending_file_name  
* Default_cred_card2_file_name

## Running the tests

Now you can run the software in one of the debug modes (see readme.md). See "things to know while testing" below. If you're using the test data, see "What to expect from the test data" below.

## Issues to know about while testing

Note that at the time of writing, *when running in debug mode* (not real mode) the debug spreadsheet is recreated every time you do a different type of reconciliation. So, for instance, if you do Bank In then Bank Out, you won’t see Bank In stuff unless you look at the spreadsheet between the two reconciliations.  

## What to expect from the test data

If you're using the test data in `reconciliation-samples/For-debugging`, this is how the data is structured / what you should expect to see.

Here are the main test data elements:
* ActualBank.csv - this contains transactions as they might be if you downloaded them from your bank.
* CredCard1.csv - this contains transactions as they might be if you downloaded them from your credit card provider.
* CredCard2.csv - the same as CredCard2.csv but mimicking a different provider and therefore formatted differently.
* CredCardInOutPending.csv - this mimics transactions you might have recorded yourself, ready to be reconciled against the transactions downloaded from the provider.
* Pending.txt - again, transactions you may have recorded yourself ready for reconciliation, but this one contains three separate sections for Bank In, Bank Out and CredCard2.

When you run the software I would suggest you start by making the following selections in this order (see ReconciliationProcess.txt for more info):
* Reconciliate!
* Load pending csvs for CredCard2, Bank In and Bank Out (from phone).  
  * This step loads the data in Pending.txt and converts it to three separate csv files, ready for the actual reconciliation.
* Reconciliate!
* Do actual reconciliation.
* Debug Mode B (remember to read the description on screen to see where your spreadsheet data will be written).

After this you want to test the four modes one at a time. I would suggest you do them in the following order:
* ActualBank and BankIn
* ActualBank and BankOut
* CredCard1 and CredCard1 InOut
* CredCard2 and CredCard2 InOut

BUDGETING MONTHS:
For each mode, it will use transactions in the test spreadsheet to calculate what your first budgeting month should be. This is based on the monthly transactions defined on Budget In and Budget Out. It finds the most recent one in the spreadsheet and then decides what your next budgeting month should be as a result. At the time of writing these are as follows:
* BankIn: previous month was Apr, so next will be May (enter 6 to budget from May to June).
* BankOut: previous month was May, so next will be June (enter 6 to budget one month June to June).
* CredCard1: previous month was June, so next will be July (enter 7 to budget one month July to July).
* CredCard2: previous month was July, so next will be Aug (enter 8 to budget one month Aug to Aug).

For Bank Out, CredCard1 and CredCard2 you'll be aked for cred card direct debit amounts - you can either choose to match these to the transactions marked "CRED CARD 1 PAYMENT DESCRIPTION ON STATEMENTS" in CredCard1.csv and CredCard2.csv, or enter different amounts to see what happens. You're only budgeting for one month (June) where cred card payments are concerned, so when asked for a second direct debit amount for each cred card, just enter 0.

(See below for what to expect from the data in each of the four modes.)

Experiment with the following:
* Reversing automatic matches as per instructions on screen. 
* Matching or ignoring the semi-automatic matches. 
* Reversing manual matches by following instructions on screen.
* Using the "Go again" functionality to match any unmatched transaction from ActualBank.
* Matching single third party transactions against multiple targets - currently used for expenses (BankIn), Amazon (CredCard2), iTunes (CredCard2) and Asda (CredCard2).

Once you're done, choose "Write csv and finish." If you're on Windows (but not on Mac) this will have the effect of writing the results to the relevant worksheets in the spreadsheet. At the time of writing, you have to check the debug spreadsheet after each of the four modes (BankIn, BankOut etc) because it will be wiped and rewritten between each mode. 

(See below for what to expect from the data in each of the four modes.)

!! Some of the behaviour described below may be slightly different if test data is out of step with the current year - the budget functionality will default some dates to the current year, which can create a discrepancy. If it's an issue, you can edit the test data to be in whatever your current year is.

### Bank In and Bank Out

ActualBank.csv contains three budgeted "monthly incoming" transactions and three "monthly outgoing" transactions. One of them may be set to be wages - you can check by looking at the test spereadsheet, on the Budget In tab. These will all be matched exactly by transactions in the spreadsheet (Your-Spreadsheet.xlsx), in the "Budget In" and "Budget Out" worksheets. This means they'll pop up at the start as "automatic matches".

MATCHING EXPENSES:
ActualBank.csv also contains transactions that are designed to match up with expenses transactions from Expected In. These will be matched as multiples when you choose the "ActualBank and BankIn" option. That is to say, multiple transactions from Expected In will be combined to match a single transaction in ActualBank.csv. You'll be given different combinations of individual transactions to choose from, with the best match at the top.

ActualBank.csv also contains Bank In transactions (positive amounts) and Bank Out transactions (negative amounts) which are designed to match up with the transactions in Pending.txt in the following ways:
* For some (eg "cheese") the date, description and amount all match exactly. These should show up at the start as "automatic matches".
* For some (eg "doughnut pumpernickel"), the description is a partial match but the date and amount are exact matches. These should also show up at the start as "automatic matches".
* For some (eg "Banana"), the date and desciption are exact matches but the amount is only a partial match - these should show up in the semi-automatic matches.
* For some (eg "BankIn transaction from ActualBank.csv"), there is no real match in Pending.txt but Pending.txt contains non-matching Bank In and Bank Out transaction with descriptions similar enough that they'll show up as semi-automatic matches.
* The credit card transactions are there because there is separate functionality that will automatically generate credit card direct debits using the data you enter on the command line (when in Bank Out mode), and the data in your config.

When you finish the reconciliation you'll see some extra "unmatched records from Bank In/Out":
* "left over from previous reconciliation" - these are from the Bank In and Bank Out worksheets in the spreadsheet.

When you finish, the latest bank balance should be written to the Totals sheet in spreadsheet.

## CredCard1 and CredCard2

CredCard1.csv and CredCard2.csv are very similar. 

They contain three "monthly transaction"s each which should match the transactions on the "Budget Out" worksheet of the spreadsheet. These should show up as "automatic matches" when you run the software.

They each contain a "CRED CARD 2 PAYMENT DESCRIPTION ON STATEMENTS" transaction which could match the data you enter on the command line (depending what you enter). 

They each contain transactions designed to match up with the transactions in Pending.txt and CredCardInOutPending.csv in the following ways:
* For some (eg "Eglantine") the date, description and amount all match exactly. These should show up at the start as "automatic matches".
* For some (eg "Fennel"), the amount and desciption are exact matches but the amount is the wrong sign - these should show up at the start as "automatic matches".
* For some (eg "Jam Kippers" / "Halloumi Icecream"), the amount and date are exact matches but the description is not a match at all - these should show up as semi-automatic matches.
* For some (eg "CredCard1 transaction from CredCard1.csv"), there is no real match in Pending.txt / CredCardInOutPending.csv but they contain non-matching CredCard1 and CredCard2 transaction with descriptions similar enough that they'll show up as semi-automatic matches.

There are deliberate duplicate transactions in CredCard1InOutPending.csv to test that duplicate transactions can still be auto-matched at the start of reconciliation.

MATCHING iTunes AND AMAZON TRANSACTIONS:
There is functionality which works the same way as the expenses functionality, to allow you to match one cred card transaction with multiple source transactions where iTunes and Amazon are concerned. It works the same way as expenses for bank in - see "MATCHING EXPENSES" above.
There is test Amazon CredCard2 data in Pending.txt which will create so many possible matches that they won't all fit on screen. You should get the option to view extra matches.

When you finish the reconciliation you'll see extra "unmatched records" labelled "left over from previous reconciliation" - these are from the CredCard1 / CredCard2 worksheets in the spreadsheet.

When you finish, the latest cred card balance should be written to the Totals sheet in spreadsheet.
