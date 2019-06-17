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
        #region Properties, member vars and constructor

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

        #endregion Properties, member vars and constructor

        #region User Instructions and Input

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

            WorkingMode working_mode = WorkingMode.DebugA;
            switch (input)
            {
                case "1": { working_mode = WorkingMode.DebugA; DebugModeA(); } break;
                case "2": { working_mode = WorkingMode.DebugB; DebugModeB(); } break;
                case "3": { working_mode = WorkingMode.DebugC; DebugModeC(); } break;
                case "4": { working_mode = WorkingMode.Real; RealMode(); } break;
            }

            DoActualReconciliation(working_mode);
        }

        public void DebugModeA()
        {
            CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.MainSpreadsheetPath, ReconConsts.MainSpreadsheetPath);
            string debug_file_path = Path.Combine(
                ReconConsts.MainSpreadsheetPath, 
                ReconConsts.BackupSubFolder, 
                ReconConsts.DebugSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(debug_file_path);
        }

        public void DebugModeB()
        {
            CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.SourceDebugSpreadsheetPath, ReconConsts.MainSpreadsheetPath);
            string debug_file_path = Path.Combine(
                ReconConsts.MainSpreadsheetPath,
                ReconConsts.BackupSubFolder,
                ReconConsts.DebugSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(debug_file_path);
        }

        public void DebugModeC()
        {
            _spreadsheetFactory = new FakeSpreadsheetRepoFactory();
        }

        private void RealMode()
        {
            CreateBackupOfRealSpreadsheet(new Clock(), ReconConsts.MainSpreadsheetPath);
            string file_path = Path.Combine(
                ReconConsts.MainSpreadsheetPath,
                ReconConsts.MainSpreadsheetFileName);
            _spreadsheetFactory = new SpreadsheetRepoFactoryFactory().GetFactory(file_path);
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

            bool using_defaults = GetAllFileDetails();

            if (!using_defaults)
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

        public bool SetFileDetailsAccordingToUserInput(string input)
        {
            bool success = true;
            _reconciliationType = ReconciliationType.Unknown;

            switch (input)
            {
                case "1":
                    {
                        success = false;
                        _reconciliationType = GetReconciliatonTypeFromUser();
                    }
                    break;
                case "2":
                    {
                        _ownedFileName = ReconConsts.DefaultCredCard1InOutFileName;
                        _thirdPartyFileName = ReconConsts.DefaultCredCard1FileName;
                        _reconciliationType = ReconciliationType.CredCard1AndCredCard1InOut;
                    }
                    break;
                case "3":
                    {
                        _ownedFileName = ReconConsts.DefaultCredCard2InOutFileName;
                        _thirdPartyFileName = ReconConsts.DefaultCredCard2FileName;
                        _reconciliationType = ReconciliationType.CredCard2AndCredCard2InOut;
                    }
                    break;
                case "4":
                    {
                        _ownedFileName = ReconConsts.DefaultBankInFileName;
                        _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                        _reconciliationType = ReconciliationType.BankAndBankIn;
                    }
                    break;
                case "5":
                    {
                        _ownedFileName = ReconConsts.DefaultBankOutFileName;
                        _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                        _reconciliationType = ReconciliationType.BankAndBankOut;
                    }
                    break;
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
            var main_file_paths = new FilePaths
            {
                MainPath = _path,
                ThirdPartyFileName = _thirdPartyFileName,
                OwnedFileName = _ownedFileName
            };

            var reconciliation_interface = LoadCorrectFiles(main_file_paths);
            reconciliation_interface?.DoTheMatching();
        }

        #endregion User Instructions and Input

        #region Debug Spreadsheet Operations

        public void CopySourceSpreadsheetToDebugSpreadsheet(string sourceSpreadsheetPath, string mainSpreadsheetPath)
        {
            string source_file_path = Path.Combine(sourceSpreadsheetPath, ReconConsts.MainSpreadsheetFileName);
            if (File.Exists(source_file_path))
            {
                string debug_file_path = Path.Combine(
                    mainSpreadsheetPath, 
                    ReconConsts.BackupSubFolder,
                    ReconConsts.DebugSpreadsheetFileName);
                File.Copy(source_file_path, debug_file_path, true);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        public void CreateBackupOfRealSpreadsheet(IClock clock, string spreadsheetPath)
        {
            string source_file_path = Path.Combine(spreadsheetPath, ReconConsts.MainSpreadsheetFileName);
            if (File.Exists(source_file_path))
            {
                string file_name_prefix = $"{ReconConsts.BackupSubFolder}\\real_backup_";
                file_name_prefix = file_name_prefix + clock.NowDateTime();
                file_name_prefix = file_name_prefix.Replace(" ", "_").Replace(":", "-").Replace("/", "-");
                string backup_file_name = file_name_prefix + "_" + ReconConsts.MainSpreadsheetFileName;
                string backup_file_path = spreadsheetPath + "\\" + backup_file_name;

                File.Copy(source_file_path, backup_file_path, true);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        public void InjectSpreadsheetFactory(ISpreadsheetRepoFactory spreadsheetFactory)
        {
            _spreadsheetFactory = spreadsheetFactory;
        }

        #endregion Debug Spreadsheet Operations

        #region File loading

        private void CreatePendingCsvs()
        {
            try
            {
                GetPath();
                var pending_csv_file_creator = new PendingCsvFileCreator(_path);
                pending_csv_file_creator.CreateAndPopulateAllCsvs();
            }
            catch (Exception e)
            {
                _inputOutput.OutputLine(e.Message);
            }
        }

        public ReconciliationInterface LoadCorrectFiles(FilePaths mainFilePaths)
        {
            _inputOutput.OutputLine("Loading data...");

            ReconciliationInterface reconciliation_interface = null;

            try
            {
                // NB This is the only function the spreadsheet is used in, until the very end (Reconciliator.Finish, called from
                // ReconciliationInterface), when another spreadsheet instance gets created by FileIO so it can call 
                // WriteBackToMainSpreadsheet. Between now and then, everything is done using csv files.
                var spreadsheet_repo = _spreadsheetFactory.CreateSpreadsheetRepo();
                var spreadsheet = new Spreadsheet(spreadsheet_repo);
                BudgetingMonths budgeting_months = RecursivelyAskForBudgetingMonths(spreadsheet);

                switch (_reconciliationType)
                {
                    case ReconciliationType.BankAndBankIn:
                        {
                            reconciliation_interface =
                                LoadBankAndBankIn(
                                    spreadsheet,
                                    budgeting_months,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.BankAndBankOut:
                        {
                            reconciliation_interface =
                                LoadBankAndBankOut(
                                    spreadsheet,
                                    budgeting_months,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.CredCard1AndCredCard1InOut:
                        {
                            reconciliation_interface =
                                LoadCredCard1AndCredCard1InOut(
                                    spreadsheet,
                                    budgeting_months,
                                    mainFilePaths);
                        }
                        break;
                    case ReconciliationType.CredCard2AndCredCard2InOut:
                        {
                            reconciliation_interface =
                                LoadCredCard2AndCredCard2InOut(
                                    spreadsheet,
                                    budgeting_months,
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

            return reconciliation_interface;
        }

        public ReconciliationInterface
            LoadBankAndBankIn(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var data_loading_info = BankAndBankInData.LoadingInfo;
            data_loading_info.FilePaths = mainFilePaths;

            var pending_file_io = new FileIO<BankRecord>(_spreadsheetFactory);
            var pending_file = new CSVFile<BankRecord>(pending_file_io);
            pending_file_io.SetFilePaths(data_loading_info.FilePaths.MainPath, data_loading_info.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pending_file.ConvertSourceLineSeparators(data_loading_info.DefaultSeparator, data_loading_info.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedBankInDataToPendingFile(budgetingMonths, pending_file, data_loading_info.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            BankAndBankIn_MergeBespokeDataWithPendingFile(_inputOutput, spreadsheet, pending_file, budgetingMonths, data_loading_info);
            _inputOutput.OutputLine("Updating source lines for output...");
            pending_file.UpdateSourceLinesForOutput(data_loading_info.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(data_loading_info.SheetName, pending_file);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.WriteToFileAsSourceLines(data_loading_info.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");
            
            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.ThirdPartyFileName);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.OwnedFileName);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.ThirdPartyDescriptor,
                data_loading_info.OwnedFileDescriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            LoadBankAndBankOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var data_loading_info = BankAndBankOutData.LoadingInfo;
            data_loading_info.FilePaths = mainFilePaths;

            var pending_file_io = new FileIO<BankRecord>(_spreadsheetFactory);
            var pending_file = new CSVFile<BankRecord>(pending_file_io);
            pending_file_io.SetFilePaths(data_loading_info.FilePaths.MainPath, data_loading_info.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pending_file.ConvertSourceLineSeparators(data_loading_info.DefaultSeparator, data_loading_info.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedBankOutDataToPendingFile(
                budgetingMonths, 
                pending_file, 
                data_loading_info.MonthlyBudgetData,
                data_loading_info.AnnualBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            BankAndBankOut_MergeBespokeDataWithPendingFile(_inputOutput, spreadsheet, pending_file, budgetingMonths, data_loading_info);
            _inputOutput.OutputLine("Updating source lines for output...");
            pending_file.UpdateSourceLinesForOutput(data_loading_info.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(data_loading_info.SheetName, pending_file);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.WriteToFileAsSourceLines(data_loading_info.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var third_party_file_io = new FileIO<ActualBankRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.ThirdPartyFileName);
            var owned_file_io = new FileIO<BankRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.OwnedFileName);
            var reconciliator = new BankReconciliator(third_party_file_io, owned_file_io, data_loading_info);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.ThirdPartyDescriptor,
                data_loading_info.OwnedFileDescriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            LoadCredCard1AndCredCard1InOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var data_loading_info = CredCard1AndCredCard1InOutData.LoadingInfo;
            data_loading_info.FilePaths = mainFilePaths;

            var pending_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheetFactory);
            var pending_file = new CSVFile<CredCard1InOutRecord>(pending_file_io);
            pending_file_io.SetFilePaths(data_loading_info.FilePaths.MainPath, data_loading_info.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pending_file.ConvertSourceLineSeparators(data_loading_info.DefaultSeparator, data_loading_info.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedCredCard1InOutDataToPendingFile(budgetingMonths, pending_file, data_loading_info.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                _inputOutput, spreadsheet, pending_file, budgetingMonths, data_loading_info);
            _inputOutput.OutputLine("Updating source lines for output...");
            pending_file.UpdateSourceLinesForOutput(data_loading_info.LoadingSeparator);
            
            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(data_loading_info.SheetName, pending_file);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.WriteToFileAsSourceLines(data_loading_info.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var third_party_file_io = new FileIO<CredCard1Record>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.ThirdPartyFileName);
            var owned_file_io = new FileIO<CredCard1InOutRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.OwnedFileName);
            var reconciliator = new CredCard1Reconciliator(third_party_file_io, owned_file_io);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.ThirdPartyDescriptor,
                data_loading_info.OwnedFileDescriptor);
            return reconciliation_interface;
        }

        public ReconciliationInterface
            LoadCredCard2AndCredCard2InOut(
                ISpreadsheet spreadsheet,
                BudgetingMonths budgetingMonths,
                FilePaths mainFilePaths)
        {
            var data_loading_info = CredCard2AndCredCard2InOutData.LoadingInfo;
            data_loading_info.FilePaths = mainFilePaths;

            var pending_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheetFactory);
            var pending_file = new CSVFile<CredCard2InOutRecord>(pending_file_io);
            pending_file_io.SetFilePaths(data_loading_info.FilePaths.MainPath, data_loading_info.PendingFileName);

            _inputOutput.OutputLine(ReconConsts.LoadingDataFromPendingFile);
            // The separator we loaded with had to match the source. Then we convert it here to match its destination.
            pending_file.Load(true, data_loading_info.DefaultSeparator);
            _inputOutput.OutputLine("Converting source line separators...");
            pending_file.ConvertSourceLineSeparators(data_loading_info.DefaultSeparator, data_loading_info.LoadingSeparator);
            _inputOutput.OutputLine(ReconConsts.MergingSomeBudgetData);
            spreadsheet.AddBudgetedCredCard2InOutDataToPendingFile(budgetingMonths, pending_file, data_loading_info.MonthlyBudgetData);
            _inputOutput.OutputLine("Merging bespoke data with pending file...");
            CredCard2AndCredCard2InOut_MergeBespokeDataWithPendingFile(
                _inputOutput, spreadsheet, pending_file, budgetingMonths, data_loading_info);
            _inputOutput.OutputLine("Updating source lines for output...");
            pending_file.UpdateSourceLinesForOutput(data_loading_info.LoadingSeparator);

            // Pending file will already exist, having already been split out from phone Notes file by a separate function call.
            // We loaded it up into memory in the previous file-specific method.
            // Then some budget amounts were added to that file (in memory).
            // Other budget amounts (like CredCard1 balance) were written directly to the spreadsheet before this too.
            // Now we load the unreconciled rows from the spreadsheet and merge them with the pending and budget data.
            // Then we write all that data away into the 'owned' csv file (eg BankOutPending.csv).
            _inputOutput.OutputLine("Merging unreconciled rows from spreadsheet with pending and budget data...");
            spreadsheet.AddUnreconciledRowsToCsvFile(data_loading_info.SheetName, pending_file);
            _inputOutput.OutputLine("Copying merged data (from pending, unreconciled, and budgeting) into main 'owned' csv file...");
            pending_file.WriteToFileAsSourceLines(data_loading_info.FilePaths.OwnedFileName);
            _inputOutput.OutputLine("...");

            var third_party_file_io = new FileIO<CredCard2Record>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.ThirdPartyFileName);
            var owned_file_io = new FileIO<CredCard2InOutRecord>(_spreadsheetFactory, data_loading_info.FilePaths.MainPath, data_loading_info.FilePaths.OwnedFileName);
            var reconciliator = new CredCard2Reconciliator(third_party_file_io, owned_file_io);
            var reconciliation_interface = new ReconciliationInterface(
                new InputOutput(),
                reconciliator,
                data_loading_info.ThirdPartyDescriptor,
                data_loading_info.OwnedFileDescriptor);
            return reconciliation_interface;
        }

        public void BankAndBankIn_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<BankRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            inputOutput.OutputLine(ReconConsts.LoadingExpenses);
            var expected_income_file_io = new FileIO<ExpectedIncomeRecord>(new FakeSpreadsheetRepoFactory());
            var expected_income_csv_file = new CSVFile<ExpectedIncomeRecord>(expected_income_file_io);
            expected_income_csv_file.Load(false);
            var expected_income_file = new ExpectedIncomeFile(expected_income_csv_file);
            spreadsheet.AddUnreconciledRowsToCsvFile<ExpectedIncomeRecord>(MainSheetNames.ExpectedIn, expected_income_file.File);
            expected_income_csv_file.PopulateSourceRecordsFromRecords();
            expected_income_file.FilterForEmployerExpensesOnly();
            expected_income_file.CopyToPendingFile(pendingFile);
            expected_income_csv_file.PopulateRecordsFromOriginalFileLoad();
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
            var most_recent_cred_card_direct_debit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                directDebitDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                credCardName,
                next_date.ToShortDateString()));
            while (input != "0")
            {
                double amount;
                if (double.TryParse(input, out amount))
                {
                    pendingFile.Records.Add(new BankRecord
                    {
                        Date = next_date,
                        Description = directDebitDescription,
                        Type = "POS",
                        UnreconciledAmount = amount
                    });
                }
                next_date = next_date.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    credCardName,
                    next_date.ToShortDateString()));
            }
        }

        public void CredCard1AndCredCard1InOut_MergeBespokeDataWithPendingFile(
                IInputOutput inputOutput,
                ISpreadsheet spreadsheet,
                ICSVFile<CredCard1InOutRecord> pendingFile,
                BudgetingMonths budgetingMonths,
                DataLoadingInformation dataLoadingInfo)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                ReconConsts.CredCard1DdDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statement_date = new DateTime();
            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard1Name,
                next_date.ToShortDateString()));
            double new_balance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out new_balance))
                {
                    pendingFile.Records.Add(new CredCard1InOutRecord
                    {
                        Date = next_date,
                        Description = ReconConsts.CredCard1RegularPymtDescription,
                        UnreconciledAmount = new_balance
                    });
                }
                statement_date = next_date.AddMonths(-1);
                next_date = next_date.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard1Name,
                    next_date.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard1Bal,
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard1Name,
                    $"{statement_date.ToString("MMM")} {statement_date.Year}"),
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
            var most_recent_cred_card_direct_debit = spreadsheet.GetMostRecentRowContainingText<BankRecord>(
                MainSheetNames.BankOut,
                ReconConsts.CredCard2DdDescription,
                new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn });

            var statement_date = new DateTime();
            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = inputOutput.GetInput(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.CredCard2Name,
                next_date.ToShortDateString()));
            double new_balance = 0;
            while (input != "0")
            {
                if (double.TryParse(input, out new_balance))
                {
                    pendingFile.Records.Add(new CredCard2InOutRecord
                    {
                        Date = next_date,
                        Description = ReconConsts.CredCard2RegularPymtDescription,
                        UnreconciledAmount = new_balance
                    });
                }
                statement_date = next_date.AddMonths(-1);
                next_date = next_date.Date.AddMonths(1);
                input = inputOutput.GetInput(string.Format(
                    ReconConsts.AskForCredCardDirectDebit,
                    ReconConsts.CredCard2Name,
                    next_date.ToShortDateString()));
            }

            spreadsheet.UpdateBalanceOnTotalsSheet(
                Codes.CredCard2Bal,
                new_balance * -1,
                string.Format(
                    ReconConsts.CredCardBalanceDescription,
                    ReconConsts.CredCard2Name,
                    $"{statement_date.ToString("MMM")} {statement_date.Year}"),
                balanceColumn: 5,
                textColumn: 6,
                codeColumn: 4);
        }

        #endregion File loading

        #region Get budgeting months

        public BudgetingMonths RecursivelyAskForBudgetingMonths(ISpreadsheet spreadsheet)
        {
            DateTime next_unplanned_month = GetNextUnplannedMonth(spreadsheet);
            int last_month_for_budget_planning = GetLastMonthForBudgetPlanning(spreadsheet, next_unplanned_month.Month);
            var budgeting_months = new BudgetingMonths
            {
                NextUnplannedMonth = next_unplanned_month.Month,
                LastMonthForBudgetPlanning = last_month_for_budget_planning,
                StartYear = next_unplanned_month.Year
            };
            if (last_month_for_budget_planning != 0)
            {
                budgeting_months.LastMonthForBudgetPlanning = ConfirmBudgetingMonthChoicesWithUser(budgeting_months, spreadsheet);
            }
            return budgeting_months;
        }

        private DateTime GetNextUnplannedMonth(ISpreadsheet spreadsheet)
        {
            DateTime default_month = DateTime.Today;
            DateTime next_unplanned_month = default_month;
            bool bad_input = false;
            try
            {
                next_unplanned_month = spreadsheet.GetNextUnplannedMonth();
            }
            catch (Exception)
            {
                string new_month = _inputOutput.GetInput(ReconConsts.CantFindMortgageRow);
                try
                {
                    if (!String.IsNullOrEmpty(new_month) && Char.IsDigit(new_month[0]))
                    {
                        int actual_month = Convert.ToInt32(new_month);
                        if (actual_month < 1 || actual_month > 12)
                        {
                            bad_input = true;
                        }
                        else
                        {
                            var year = default_month.Year;
                            if (actual_month < default_month.Month)
                            {
                                year++;
                            }
                            next_unplanned_month = new DateTime(year, actual_month, 1);
                        }
                    }
                    else
                    {
                        bad_input = true;
                    }
                }
                catch (Exception)
                {
                    bad_input = true;
                }
            }

            if (bad_input)
            {
                _inputOutput.OutputLine(ReconConsts.DefaultUnplannedMonth);
                next_unplanned_month = default_month;
            }

            return next_unplanned_month;
        }

        private int GetLastMonthForBudgetPlanning(ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nextUnplannedMonth);
            var request_to_enter_month = String.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            string month = _inputOutput.GetInput(request_to_enter_month);
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
            var new_result = budgetingMonths.LastMonthForBudgetPlanning;
            string input = GetResponseToBudgetingMonthsConfirmationMessage(budgetingMonths);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                new_result = budgetingMonths.LastMonthForBudgetPlanning;
            }
            else
            {
                // Recursion ftw!
                new_result = GetLastMonthForBudgetPlanning(spreadsheet, budgetingMonths.NextUnplannedMonth);
            }

            return new_result;
        }

        private string GetResponseToBudgetingMonthsConfirmationMessage(BudgetingMonths budgetingMonths)
        {
            string first_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.NextUnplannedMonth);
            string second_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgetingMonths.LastMonthForBudgetPlanning);

            int month_span = budgetingMonths.NumBudgetingMonths();

            var confirmation_text = String.Format(ReconConsts.ConfirmMonthInterval, first_month, second_month, month_span);

            return _inputOutput.GetInput(confirmation_text);
        }

        private int HandleZeroMonthChoiceResult(int chosenMonth, ISpreadsheet spreadsheet, int nextUnplannedMonth)
        {
            var new_result = chosenMonth;
            if (chosenMonth == 0)
            {
                var input = _inputOutput.GetInput(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    new_result = 0;
                    _inputOutput.OutputLine(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    new_result = GetLastMonthForBudgetPlanning(spreadsheet, nextUnplannedMonth);
                }
            }
            return new_result;
        }

        #endregion Get budgeting months
    }
}
