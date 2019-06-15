using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation
{
    internal class ReconciliationIntro
    {
        private string _path = "";
        private string _thirdPartyFileName = "";
        private string _ownedFileName = "";
        private ReconciliationType _reconciliationType;

        private ISpreadsheetRepoFactory _spreadsheetFactory = new FakeSpreadsheetRepoFactory();
        private readonly IInputOutput _inputOutput;

        public ReconciliationIntro(IInputOutput inputOutput)
        {
            _inputOutput = inputOutput;
        }

        public void Start()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputOptions(new List<string>
            {
                ReconConsts.LoadPendingCsvs,
                "2. Do actual reconciliation."
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.PendingOrReconciliate);

            switch (input)
            {
                case "1": CreatePendingCsvs(); break;
                case "2": DecideOnDebug(); break;
            }
        }

        public void DecideOnDebug()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputOptions(new List<string>
            {
                $"1. Debug Mode A: Copy live sheet to debug version in [live location]/{ReconConsts.BackupSubFolder}, and work on it from there.",
                $"2. Debug Mode B: Copy sheet from {ReconConsts.SourceDebugSpreadsheetPath} to [live location]/{ReconConsts.BackupSubFolder}, and work on it from there.",
                "3. Debug Mode C: Use fake spreadsheet repo (like you would get in .Net Core).",
                "4. Work in REAL mode"
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.DebugOrReal);

            WorkingMode workingMode = WorkingMode.DebugA;
            switch (input)
            {
                case "1": { workingMode = WorkingMode.DebugA; DebugModeA(); } break;
                case "2": { workingMode = WorkingMode.DebugB; DebugModeB(); } break;
                case "3": { workingMode = WorkingMode.DebugC; DebugModeC(); } break;
                case "4": { workingMode = WorkingMode.Real; RealMode(); } break;
            }

            DoActualReconciliation(workingMode);
        }

        public void DebugModeA()
        {
            CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.MainSpreadsheetPath, ReconConsts.MainSpreadsheetPath);
            string debugFilePath = Path.Combine(
                ReconConsts.MainSpreadsheetPath, 
                ReconConsts.BackupSubFolder, 
                ReconConsts.DebugSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(debugFilePath);
        }

        public void DebugModeB()
        {
            CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.SourceDebugSpreadsheetPath, ReconConsts.MainSpreadsheetPath);
            string debugFilePath = Path.Combine(
                ReconConsts.MainSpreadsheetPath,
                ReconConsts.BackupSubFolder,
                ReconConsts.DebugSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(debugFilePath);
        }

        public void DebugModeC()
        {
            _spreadsheetFactory = new FakeSpreadsheetRepoFactory();
        }

        private void RealMode()
        {
            CreateBackupOfRealSpreadsheet(new Clock(), ReconConsts.MainSpreadsheetPath);
            string filePath = Path.Combine(
                ReconConsts.MainSpreadsheetPath,
                ReconConsts.MainSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(filePath);
        }

        public void CopySourceSpreadsheetToDebugSpreadsheet(string sourceSpreadsheetPath, string mainSpreadsheetPath)
        {
            string sourceFilePath = Path.Combine(sourceSpreadsheetPath, ReconConsts.MainSpreadsheetFileName);
            if (File.Exists(sourceFilePath))
            {
                string debugFilePath = Path.Combine(
                    mainSpreadsheetPath, 
                    ReconConsts.BackupSubFolder,
                    ReconConsts.DebugSpreadsheetFileName);
                File.Copy(sourceFilePath, debugFilePath, true);
            }
            else
            {
                throw new Exception($"Can't find file: {sourceFilePath}");
            }
        }

        public void CreateBackupOfRealSpreadsheet(IClock clock, string spreadsheetPath)
        {
            string sourceFilePath = Path.Combine(spreadsheetPath, ReconConsts.MainSpreadsheetFileName);
            if (File.Exists(sourceFilePath))
            {
                string fileNamePrefix = $"{ReconConsts.BackupSubFolder}\\real_backup_";
                fileNamePrefix = fileNamePrefix + clock.NowDateTime();
                fileNamePrefix = fileNamePrefix.Replace(" ", "_").Replace(":", "-").Replace("/", "-");
                string backupFileName = fileNamePrefix + "_" + ReconConsts.MainSpreadsheetFileName;
                string backupFilePath = spreadsheetPath + "\\" + backupFileName;

                File.Copy(sourceFilePath, backupFilePath, true);
            }
            else
            {
                throw new Exception($"Can't find file: {sourceFilePath}");
            }
        }

        public void InjectSpreadsheetFactory(ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _spreadsheetFactory = spreadsheetFactory;
        }

        public void DoActualReconciliation(WorkingMode workingMode)
        {
            try
            {
                ShowInstructions(workingMode);
                GetPathAndFileNames();
                DoMatching();
            }
            catch (Exception exception)
            {
                if (exception.Message.ToUpper() == "EXIT")
                {
                    _inputOutput.OutputLine("Taking you back to the main screen so you can start again if you want.");
                }
                else
                {
                    _inputOutput.ShowError(exception);
                }
            }
        }

        private void DoMatching()
        {
            var mainFilePaths = new FilePaths
            {
                MainPath = _path,
                ThirdPartyFileName = _thirdPartyFileName,
                OwnedFileName = _ownedFileName
            };

            var reconciliationInterface = LoadCorrectFiles(mainFilePaths);
            reconciliationInterface?.DoTheMatching();
        }

        private void CreatePendingCsvs()
        {
            try
            {
                GetPath();
                var pendingCsvFileCreator = new PendingCsvFileCreator(_path);
                pendingCsvFileCreator.CreateAndPopulateAllCsvs();
            }
            catch (Exception e)
            {
                _inputOutput.OutputLine(e.Message);
            }
        }

        private void ShowInstructions(WorkingMode workingMode)
        {
            _inputOutput.OutputLine("Here's how it works:");
            _inputOutput.OutputLine("                                      ****");
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_01);
            _inputOutput.OutputLine("                                      ****");
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_02);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_03);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_04);
            _inputOutput.OutputLine("                                      ****");
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_05);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_06);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_07);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_08);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_09);
            _inputOutput.OutputLine("                                      ****");
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_10);
            _inputOutput.OutputLine(ReconConsts.Instructions_Line_11);
            _inputOutput.OutputLine("");

            switch (workingMode)
            {
                case WorkingMode.DebugA: ShowDebugADataMessage(); break;
                case WorkingMode.DebugB: ShowDebugBDataMessage(); break;
                case WorkingMode.DebugC: ShowDebugCDataMessage(); break;
                case WorkingMode.Real: ShowRealDataMessage(); break;
            }
        }

        private void ShowDebugADataMessage()
        {
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***                    WORKING IN DEBUG MODE A!                     ***");
            _inputOutput.OutputLine("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("*** It's an up to date copy of the main spreadsheet. It lives here: ***");
            _inputOutput.OutputLine($"***  {ReconConsts.MainSpreadsheetPath}/{ReconConsts.BackupSubFolder}  ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***  You can find debug versions of all csv files and a spreadsheet ***");
            _inputOutput.OutputLine($"***     in [project root]/reconciliation-samples/For debugging     ***"); 
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("");
        }

        private void ShowDebugBDataMessage()
        {
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***                    WORKING IN DEBUG MODE B!                     ***");
            _inputOutput.OutputLine("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***      We'll copy the spreadsheet from the following folder:      ***");
            _inputOutput.OutputLine($"***                {ReconConsts.SourceDebugSpreadsheetPath}                 ***");
            _inputOutput.OutputLine("***              The working copy will be placed here:              ***");
            _inputOutput.OutputLine($"***  {ReconConsts.MainSpreadsheetPath}/{ReconConsts.BackupSubFolder}  ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***  You can find debug versions of all csv files and a spreadsheet ***");
            _inputOutput.OutputLine($"***     in [project root]/reconciliation-samples/For debugging     ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("");
        }

        private void ShowDebugCDataMessage()
        {
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***                    WORKING IN DEBUG MODE C!                     ***");
            _inputOutput.OutputLine("***                  NO SPREADSHEET WILL BE USED!                   ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***                 Using fake spreadsheet factory.                 ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("***********************************************************************");
            _inputOutput.OutputLine("");
        }

        private void ShowRealDataMessage()
        {
            _inputOutput.OutputLine("***********************************************************************************");
            _inputOutput.OutputLine("***********************************************************************************");
            _inputOutput.OutputLine("***                                                                             ***");
            _inputOutput.OutputLine("***                            WORKING IN REAL MODE!                            ***");
            _inputOutput.OutputLine("***                      REAL SPREADSHEET WILL BE UPDATED!                      ***");
            _inputOutput.OutputLine("***                                                                             ***");
            _inputOutput.OutputLine("*** (unless you're in .Net Core, in which case you're in debug mode by default) ***");
            _inputOutput.OutputLine("***                                                                             ***");
            _inputOutput.OutputLine("***********************************************************************************");
            _inputOutput.OutputLine("***********************************************************************************");
            _inputOutput.OutputLine("");
        }

        private ReconciliationType GetReconciliatonTypeFromUser()
        {
            ReconciliationType result = ReconciliationType.Unknown;

            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("What type are your third party and owned files?");
            _inputOutput.OutputOptions(new List<string>
            {
                ReconConsts.Accounting_Type_01,
                ReconConsts.Accounting_Type_02,
                ReconConsts.Accounting_Type_03,
                ReconConsts.Accounting_Type_04,
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.FourAccountingTypes);

            switch (input)
            {
                case "1": result = ReconciliationType.CredCard1AndCredCard1InOut; break;
                case "2": result = ReconciliationType.CredCard2AndCredCard2InOut; break;
                case "3": result = ReconciliationType.BankAndBankIn; break;
                case "4": result = ReconciliationType.BankAndBankOut; break;
            }

            return result;
        }

        private void GetPathAndFileNames()
        {
            _inputOutput.OutputLine("Mathematical dude! Let's do some reconciliating. Type Exit at any time to leave (although to be honest I'm not sure that actually works...)");

            bool usingDefaults = GetAllFileDetails();

            if (!usingDefaults)
            {
                GetPath();
                GetThirdPartyFileName();
                GetOwnedFileName();
            }
        }

        private void GetPath()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("Would you like to enter a file path or use the default?");
            _inputOutput.OutputOptions(new List<string>
            {
                "1. Enter a path",
                $"2. Use default ({ReconConsts.DefaultFilePath})"
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.PathOrDefault);

            switch (input)
            {
                case "1": _path = _inputOutput.GetInput(ReconConsts.EnterCsvPath); break;
                case "2": _path = ReconConsts.DefaultFilePath; break;
            }
        }

        private void GetThirdPartyFileName()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("Would you like to enter a file name for your third party csv file, or use a default?");
            _inputOutput.OutputOptions(new List<string>
            {
                "1. Enter a file name",
                string.Format(ReconConsts.File_Name_Option_02, ReconConsts.DefaultCredCard1FileName),
                string.Format(ReconConsts.File_Name_Option_03, ReconConsts.DefaultCredCard2FileName),
                string.Format(ReconConsts.File_Name_Option_04, ReconConsts.DefaultBankFileName)
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.FourFileNameOptions);

            switch (input)
            {
                case "1": _thirdPartyFileName = _inputOutput.GetInput(ReconConsts.EnterThirdPartyFileName); break;
                case "2": _thirdPartyFileName = ReconConsts.DefaultCredCard1FileName; break;
                case "3": _thirdPartyFileName = ReconConsts.DefaultCredCard2FileName; break;
                case "4": _thirdPartyFileName = ReconConsts.DefaultBankFileName; break;
            }
        }

        private bool GetAllFileDetails()
        {
            bool success = true;

            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("Would you like to enter your own file details, or use defaults?");
            _inputOutput.OutputOptions(new List<string>
            {
                "1. Enter my own file details",
                ReconConsts.File_Details_02,
                ReconConsts.File_Details_03,
                ReconConsts.File_Details_04,
                ReconConsts.File_Details_05,
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.FiveFileDetails);

            success = SetFileDetailsAccordingToUserInput(input);

            return success;
        }

        public bool SetFileDetailsAccordingToUserInput(string input)
        {
            bool success = true;
            _reconciliationType = ReconciliationType.Unknown;

            switch (input)
            {
                case "1": {
                    success = false;
                    _reconciliationType = GetReconciliatonTypeFromUser();
                } break;
                case "2": {
                    _ownedFileName = ReconConsts.DefaultCredCard1InOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultCredCard1FileName;
                    _reconciliationType = ReconciliationType.CredCard1AndCredCard1InOut;
                } break;
                case "3": {
                    _ownedFileName = ReconConsts.DefaultCredCard2InOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultCredCard2FileName;
                    _reconciliationType = ReconciliationType.CredCard2AndCredCard2InOut;
                } break;
                case "4":
                {
                    _ownedFileName = ReconConsts.DefaultBankInFileName;
                    _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                    _reconciliationType = ReconciliationType.BankAndBankIn;
                } break;
                case "5":
                {
                    _ownedFileName = ReconConsts.DefaultBankOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                    _reconciliationType = ReconciliationType.BankAndBankOut;
                } break;
            }

            if (success)
            {
                _path = ReconConsts.DefaultFilePath;
                _inputOutput.OutputLine("You are using the following defaults:");
                _inputOutput.OutputLine("File path will be " + _path);
                _inputOutput.OutputLine("Third party file name will be " + _thirdPartyFileName + ".csv");
                _inputOutput.OutputLine("Owned file name will be " + _ownedFileName + ".csv");
            }

            return success;
        }

        private void GetOwnedFileName()
        {
            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("Would you like to enter a file name for your own csv file, or use a default?");
            _inputOutput.OutputOptions(new List<string>
            {
                "1. Enter a file name",
                string.Format(ReconConsts.File_Option_02, ReconConsts.DefaultCredCard1InOutFileName),
                string.Format(ReconConsts.File_Option_03, ReconConsts.DefaultCredCard2InOutFileName),
                string.Format(ReconConsts.File_Option_04, ReconConsts.DefaultBankInFileName),
                string.Format(ReconConsts.File_Option_05, ReconConsts.DefaultBankOutFileName),
            });

            string input = _inputOutput.GetGenericInput(ReconConsts.FiveFileOptions);

            CaptureOwnedFileName(input);
        }

        private void CaptureOwnedFileName(string input)
        {
            switch (input)
            {
                case "1":
                    {
                        _ownedFileName = _inputOutput.GetInput(ReconConsts.EnterOwnedFileName);
                    }
                    break;
                case "2":
                    {
                        _ownedFileName = ReconConsts.DefaultCredCard1InOutFileName;
                    }
                    break;
                case "3":
                    {
                        _ownedFileName = ReconConsts.DefaultCredCard2InOutFileName;
                    }
                    break;
                case "4":
                    {
                        _ownedFileName = ReconConsts.DefaultBankInFileName;
                    }
                    break;
                case "5":
                    {
                        _ownedFileName = ReconConsts.DefaultBankOutFileName;
                    }
                    break;
            }
        }

        public ReconciliationInterface LoadCorrectFiles(FilePaths mainFilePaths)
        {
            _inputOutput.OutputLine("Loading data...");

            ReconciliationInterface reconciliationInterface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheetRepo = _spreadsheetFactory.CreateSpreadsheetRepo();
                var spreadsheet = new Spreadsheet(spreadsheetRepo);
                BudgetingMonths budgetingMonths = RecursivelyAskForBudgetingMonths(spreadsheet);

                switch (_reconciliationType)
                {
                    case ReconciliationType.BankAndBankIn:
                        {
                            reconciliationInterface =
                                LoadBankAndBankIn(
                                    spreadsheet,
                                    budgetingMonths,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.BankAndBankOut:
                        {
                            reconciliationInterface =
                                LoadBankAndBankOut(
                                    spreadsheet,
                                    budgetingMonths,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.CredCard1AndCredCard1InOut:
                        {
                            reconciliationInterface =
                                LoadCredCard1AndCredCard1InOut(
                                    spreadsheet,
                                    budgetingMonths,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.CredCard2AndCredCard2InOut:
                        {
                            reconciliationInterface =
                                LoadCredCard2AndCredCard2InOut(
                                    spreadsheet,
                                    budgetingMonths,
                                    mainFilePaths);
                        }
                        break;
                    default:
                        {
                            _inputOutput.OutputLine("I don't know what files to load! Terminating now.");
                        }
                        break;
                }
            }
            finally
            {
                _spreadsheetFactory.DisposeOfSpreadsheetRepo();
            }

            _inputOutput.OutputLine("");
            _inputOutput.OutputLine("");

            return reconciliationInterface;
        }

        public ReconciliationInterface
            LoadBankAndBankIn(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var dataLoadingInfo = BankAndBankInData.LoadingInfo;
            dataLoadingInfo.FilePaths = mainFilePaths;

            var pendingFileIO = new FileIO<BankRecord>(_spreadsheetFactory);
            var pendingFile = new CSVFile<BankRecord>(pendingFileIO);
            pendingFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pendingFile.Load(true, dataLoadingInfo.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pendingFile.ConvertSourceLineSeparators(dataLoadingInfo.DefaultSeparator, dataLoadingInfo.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedBankInDataToPendingFile(budgetingMonths, pendingFile, dataLoadingInfo.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            BankAndBankIn_MergeBespokeDataWithPendingFile(_inputOutput, spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            _inputOutput.OutputLine("Updating source lines for output...");
            pendingFile.UpdateSourceLinesForOutput(dataLoadingInfo.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(dataLoadingInfo.SheetName, pendingFile);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pendingFile.WriteToFileAsSourceLines(dataLoadingInfo.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");
            
            var thirdPartyFileIO = new FileIO<ActualBankRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.ThirdPartyFileName);
            var ownedFileIO = new FileIO<BankRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.OwnedFileName);
            var reconciliator = new BankReconciliator(thirdPartyFileIO, ownedFileIO, dataLoadingInfo);
            var reconciliationInterface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                dataLoadingInfo.ThirdPartyDescriptor,
                dataLoadingInfo.OwnedFileDescriptor);
            return reconciliationInterface;
        }

        public ReconciliationInterface
            LoadBankAndBankOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var dataLoadingInfo = BankAndBankOutData.LoadingInfo;
            dataLoadingInfo.FilePaths = mainFilePaths;

            var pendingFileIO = new FileIO<BankRecord>(_spreadsheetFactory);
            var pendingFile = new CSVFile<BankRecord>(pendingFileIO);
            pendingFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pendingFile.Load(true, dataLoadingInfo.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pendingFile.ConvertSourceLineSeparators(dataLoadingInfo.DefaultSeparator, dataLoadingInfo.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedBankOutDataToPendingFile(
                budgetingMonths, 
                pendingFile, 
                dataLoadingInfo.MonthlyBudgetData,
                dataLoadingInfo.AnnualBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            BankAndBankOut_MergeBespokeDataWithPendingFile(_inputOutput, spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            _inputOutput.OutputLine("Updating source lines for output...");
            pendingFile.UpdateSourceLinesForOutput(dataLoadingInfo.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(dataLoadingInfo.SheetName, pendingFile);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pendingFile.WriteToFileAsSourceLines(dataLoadingInfo.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var thirdPartyFileIO = new FileIO<ActualBankRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.ThirdPartyFileName);
            var ownedFileIO = new FileIO<BankRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.OwnedFileName);
            var reconciliator = new BankReconciliator(thirdPartyFileIO, ownedFileIO, dataLoadingInfo);
            var reconciliationInterface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                dataLoadingInfo.ThirdPartyDescriptor,
                dataLoadingInfo.OwnedFileDescriptor);
            return reconciliationInterface;
        }

        public ReconciliationInterface
            LoadCredCard1AndCredCard1InOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var dataLoadingInfo = CredCard1AndCredCard1InOutData.LoadingInfo;
            dataLoadingInfo.FilePaths = mainFilePaths;

            var pendingFileIO = new FileIO<CredCard1InOutRecord>(_spreadsheetFactory);
            var pendingFile = new CSVFile<CredCard1InOutRecord>(pendingFileIO);
            pendingFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pendingFile.Load(true, dataLoadingInfo.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pendingFile.ConvertSourceLineSeparators(dataLoadingInfo.DefaultSeparator, dataLoadingInfo.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budgetingMonths, pendingFile, dataLoadingInfo.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                _inputOutput, spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            _inputOutput.OutputLine("Updating source lines for output...");
            pendingFile.UpdateSourceLinesForOutput(dataLoadingInfo.LoadingSeparator);
            
            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(dataLoadingInfo.SheetName, pendingFile);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pendingFile.WriteToFileAsSourceLines(dataLoadingInfo.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var thirdPartyFileIO = new FileIO<CredCard1Record>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.ThirdPartyFileName);
            var ownedFileIO = new FileIO<CredCard1InOutRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.OwnedFileName);
            var reconciliator = new CredCard1Reconciliator(thirdPartyFileIO, ownedFileIO);
            var reconciliationInterface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                dataLoadingInfo.ThirdPartyDescriptor,
                dataLoadingInfo.OwnedFileDescriptor);
            return reconciliationInterface;
        }

        public ReconciliationInterface
            LoadCredCard2AndCredCard2InOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var dataLoadingInfo = CredCard2AndCredCard2InOutData.LoadingInfo;
            dataLoadingInfo.FilePaths = mainFilePaths;

            var pendingFileIO = new FileIO<CredCard2InOutRecord>(_spreadsheetFactory);
            var pendingFile = new CSVFile<CredCard2InOutRecord>(pendingFileIO);
            pendingFileIO.SetFilePaths(dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pendingFile.Load(true, dataLoadingInfo.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pendingFile.ConvertSourceLineSeparators(dataLoadingInfo.DefaultSeparator, dataLoadingInfo.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budgetingMonths, pendingFile, dataLoadingInfo.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            CredCard2AndCredCard2InOut_MergeBespokeDataWithPendingFile(
                _inputOutput, spreadsheet, pendingFile, budgetingMonths, dataLoadingInfo);
            _inputOutput.OutputLine("Updating source lines for output...");
            pendingFile.UpdateSourceLinesForOutput(dataLoadingInfo.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(dataLoadingInfo.SheetName, pendingFile);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pendingFile.WriteToFileAsSourceLines(dataLoadingInfo.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var thirdPartyFileIO = new FileIO<CredCard2Record>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.ThirdPartyFileName);
            var ownedFileIO = new FileIO<CredCard2InOutRecord>(_spreadsheetFactory, dataLoadingInfo.FilePaths.MainPath, dataLoadingInfo.FilePaths.OwnedFileName);
            var reconciliator = new CredCard2Reconciliator(thirdPartyFileIO, ownedFileIO);
            var reconciliationInterface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                dataLoadingInfo.ThirdPartyDescriptor,
                dataLoadingInfo.OwnedFileDescriptor);
            return reconciliationInterface;
        }

        public BudgetingMonths RecursivelyAskForBudgetingMonths(ISpreadsheet spreadsheet)
        {
            DateTime nextUnplannedMonth = GetNextUnplannedMonth(spreadsheet);
            int lastMonthForBudgetPlanning = GetLastMonthForBudgetPlanning(spreadsheet, nextUnplannedMonth.Month);
            var budgetingMonths = new BudgetingMonths
            {
                NextUnplannedMonth = nextUnplannedMonth.Month,
                LastMonthForBudgetPlanning = lastMonthForBudgetPlanning,
                StartYear = nextUnplannedMonth.Year
            };
            if (lastMonthForBudgetPlanning != 0)
            {
                budgetingMonths.LastMonthForBudgetPlanning = ConfirmBudgetingMonthChoicesWithUser(budgetingMonths, spreadsheet);
            }
            return budgetingMonths;
        }

        public void BankAndBankIn_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<BankRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            inputOutput.OutputLine(ReconConsts.LoadingExpenses);
            var expectedIncomeFileIO = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expectedIncomeCSVFile = new CSVFile<ExpectedIncomeRecord>(expectedIncomeFileIO);
            expectedIncomeCSVFile.Load(false);
            var expectedIncomeFile = new ExpectedIncomeFile(expectedIncomeCSVFile);
            spreadsheet.AddUnreconciledRowsToCsvFile<ExpectedIncomeRecord>(MainSheetNames.ExpectedIn, expectedIncomeFile.File);
            expectedIncomeCSVFile.PopulateSourceRecordsFromRecords();
            expectedIncomeFile.FilterForEmployerExpensesOnly();
            expectedIncomeFile.CopyToPendingFile(pendingFile);
            expectedIncomeCSVFile.PopulateRecordsFromOriginalFileLoad();
        }

        public void BankAndBankOut_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<BankRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            BankAndBankOut_AddMostRecentCreditCardDirectDebits(
                inputOutput,
                spreadsheet,
                pendingFile,
                ReconConsts.CredCard1Name,
                ReconConsts.CredCard1DdDescription);

            BankAndBankOut_AddMostRecentCreditCardDirectDebits(
                inputOutput,
                spreadsheet,
                (ICSVFile<BankRecord>)pendingFile,
                ReconConsts.CredCard2Name,
                ReconConsts.CredCard2DdDescription);
        }

        private void BankAndBankOut_AddMostRecentCreditCardDirectDebits(
            IInputOutput inputOutput,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pendingFile,
            string credCardName,
            string directDebitDescription)
        {
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                directDebitDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                credCardName,
                nextDate.ToShortDateString()));
            while (input != "0")
            {
                double amount;
                if (double.TryParse(input, out amount))
                {
                    pendingFile.Records.Add(new BankRecord
                    {
                        Date = nextDate,
                        Description = directDebitDescription,
                        Type = "POS",
                        UnreconciledAmount = amount
                    });
                }
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    credCardName,
                    nextDate.ToShortDateString()));
            }
        }

        public void CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<CredCard1InOutRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                ReconConsts.CredCard1DdDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statementDate = new DateTime();
            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard1Name,
                nextDate.ToShortDateString()));
            double newBalance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out newBalance))
                {
                    pendingFile.Records.Add(new CredCard1InOutRecord
                    {
                        Date = nextDate,
                        Description = ReconConsts.CredCard1RegularPymtDescription,
                        UnreconciledAmount = newBalance
                    });
                }
                statementDate = nextDate.AddMonths(-1);
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard1Name,
                    nextDate.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard1Bal,
                newBalance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard1Name,
                    $"{statementDate.ToString("MMM")} {statementDate.Year}"),
                balanceColumn: 5,
                textColumn: 6,
                codeColumn: 4);
        }

        public void CredCard2AndCredCard2InOut_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<CredCard2InOutRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            var mostRecentCredCardDirectDebit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                ReconConsts.CredCard2DdDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statementDate = new DateTime();
            var nextDate = mostRecentCredCardDirectDebit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard2Name,
                nextDate.ToShortDateString()));
            double newBalance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out newBalance))
                {
                    pendingFile.Records.Add(new CredCard2InOutRecord
                    {
                        Date = nextDate,
                        Description = ReconConsts.CredCard2RegularPymtDescription,
                        UnreconciledAmount = newBalance
                    });
                }
                statementDate = nextDate.AddMonths(-1);
                nextDate = nextDate.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard2Name,
                    nextDate.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard2Bal,
                newBalance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard2Name,
                    $"{statementDate.ToString("MMM")} {statementDate.Year}"),
                balanceColumn: 5,
                textColumn: 6,
                codeColumn: 4);
        }

        private DateTime GetNextUnplannedMonth(ISpreadsheet spreadsheet)
        {
            DateTime defaultMonth = DateTime.Today;
            DateTime nextUnplannedMonth = defaultMonth;
            bool badInput = false;
            try
            {
                nextUnplannedMonth = spreadsheet.GetNextUnplannedMonth();
            }
            catch (Exception)
            {
                string newMonth = _inputOutput.GetInput(ReconConsts.CantFindMortgageRow);
                try
                {
                    if (!String.IsNullOrEmpty(newMonth) && Char.IsDigit(newMonth[0]))
                    {
                        int actualMonth = Convert.ToInt32(newMonth);
                        if (actualMonth < 1 || actualMonth > 12)
                        {
                            badInput = true;
                        }
                        else
                        {
                            var year = defaultMonth.Year;
                            if (actualMonth < defaultMonth.Month)
                            {
                                year++;
                            }
                            nextUnplannedMonth = new DateTime(year, actualMonth, 1);
                        }
                    }
                    else
                    {
                        badInput = true;
                    }
                }
                catch (Exception)
                {
                    badInput = true;
                }
            }

            if (badInput)
            {
                _inputOutput.OutputLine(ReconConsts.DefaultUnplannedMonth);
                nextUnplannedMonth = defaultMonth;
            }

            return nextUnplannedMonth;
        }

        private int GetLastMonthForBudgetPlanning(ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            string nextUnplannedMonthAsString = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nextUnplannedMonth);
            var requestToEnterMonth = String.Format(ReconConsts.EnterMonths, nextUnplannedMonthAsString);
            string month = _inputOutput.GetInput(requestToEnterMonth);
            int result = 0;

            try
            {
                if (!String.IsNullOrEmpty(month) && Char.IsDigit(month[0]))
                {
                    result = Convert.ToInt32(month);
                    if (result < 1 || result > 12)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore it and return zero by default.
            }

            result = HandleZeroMonthChoiceResult(result, spreadsheet, nextUnplannedMonth);
            return result;
        }

        private int ConfirmBudgetingMonthChoicesWithUser(BudgetingMonths budgetingMonths, ISpreadsheet spreadsheet)
        {
            var newResult = budgetingMonths.LastMonthForBudgetPlanning;
            string input = GetResponseToBudgetingMonthsConfirmationMessage(budgetingMonths);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                newResult = budgetingMonths.LastMonthForBudgetPlanning;
            }
            else
            {
                // Recursion ftw!
                newResult = GetLastMonthForBudgetPlanning(spreadsheet, budgetingMonths.NextUnplannedMonth);
            }

            return newResult;
        }

        private string GetResponseToBudgetingMonthsConfirmationMessage(BudgetingMonths budgetingMonths)
        {
            string firstMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.NextUnplannedMonth);
            string secondMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.LastMonthForBudgetPlanning);

            int monthSpan = budgetingMonths.NumBudgetingMonths();

            var confirmationText = String.Format(ReconConsts.ConfirmMonthInterval, firstMonth, secondMonth, monthSpan);

            return _inputOutput.GetInput(confirmationText);
        }

        private int HandleZeroMonthChoiceResult(int chosenMonth, ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            var newResult = chosenMonth;
            if (chosenMonth == 0)
            {
                var input = _inputOutput.GetInput(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    newResult = 0;
                    _inputOutput.OutputLine(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    newResult = GetLastMonthForBudgetPlanning(spreadsheet, nextUnplannedMonth);
                }
            }
            return newResult;
        }
    }
}
