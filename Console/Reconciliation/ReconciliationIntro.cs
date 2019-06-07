using System;
using System.Collections.Generic;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Matchers;
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

        private ISpreadsheetRepoFactory _spreadsheetFactory = new FakeSpreadsheetRepoFactory();
        private readonly IInputOutput _inputOutput;
        private IMatcher _matcher = null;

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
            _matcher.DoMatching(mainFilePaths);
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
            _inputOutput.OutputLine("***      in [project root]/reconciliation-samples/For debugging     ***");
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
            _inputOutput.OutputLine($"***        {ReconConsts.MainSpreadsheetPath}/{ReconConsts.BackupSubFolder}        ***");
            _inputOutput.OutputLine("***                                                                 ***");
            _inputOutput.OutputLine("***  You can find debug versions of all csv files and a spreadsheet ***");
            _inputOutput.OutputLine("***      in [project root]/reconciliation-samples/For debugging     ***");
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

        private IMatcher GetReconciliatonTypeFromUser()
        {
            IMatcher result = null;

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
                case "1": result = new CredCard1AndCredCard1InOutMatcher(_inputOutput, _spreadsheetFactory); break;
                case "2": result = new CredCard2AndCredCard2InOutMatcher(_inputOutput, _spreadsheetFactory); break;
                case "3": result = new BankAndBankInMatcher(_inputOutput, _spreadsheetFactory, new BankAndBankInLoader(_spreadsheetFactory)); break;
                case "4": result = new BankAndBankOutMatcher(_inputOutput, _spreadsheetFactory); break;
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

        private bool SetFileDetailsAccordingToUserInput(string input)
        {
            bool success = true;
            _matcher = null;

            switch (input)
            {
                case "1": {
                    success = false;
                    _matcher = GetReconciliatonTypeFromUser();
                } break;
                case "2": {
                    _ownedFileName = ReconConsts.DefaultCredCard1InOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultCredCard1FileName;
                    _matcher = new CredCard1AndCredCard1InOutMatcher(_inputOutput, _spreadsheetFactory);
                } break;
                case "3": {
                    _ownedFileName = ReconConsts.DefaultCredCard2InOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultCredCard2FileName;
                    _matcher = new CredCard2AndCredCard2InOutMatcher(_inputOutput, _spreadsheetFactory);
                } break;
                case "4":
                {
                    _ownedFileName = ReconConsts.DefaultBankInFileName;
                    _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                    _matcher = new BankAndBankInMatcher(_inputOutput, _spreadsheetFactory, new BankAndBankInLoader(_spreadsheetFactory));
                } break;
                case "5":
                {
                    _ownedFileName = ReconConsts.DefaultBankOutFileName;
                    _thirdPartyFileName = ReconConsts.DefaultBankFileName;
                    _matcher = new BankAndBankOutMatcher(_inputOutput, _spreadsheetFactory);
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
    }
}
