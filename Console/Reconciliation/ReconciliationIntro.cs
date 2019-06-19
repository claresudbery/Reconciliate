using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Loaders;
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
        private string _third_party_file_name = "";
        private string _owned_file_name = "";
        private ReconciliationType _reconciliation_type;

        private ISpreadsheetRepoFactory _spreadsheet_factory = new FakeSpreadsheetRepoFactory();
        private readonly IInputOutput _input_output;
        private readonly FileLoader _file_loader;

        public ReconciliationIntro(IInputOutput input_output)
        {
            _input_output = input_output;
            _file_loader = new FileLoader(_input_output, _spreadsheet_factory);
        }

        #endregion Properties, member vars and constructor

        #region User Instructions and Input

        public void Start()
        {
            _input_output.Output_line("");
            _input_output.Output_options(new List<string>
            {
                ReconConsts.Load_pending_csvs,
                "2. Do actual reconciliation."
            });

            string input = _input_output.Get_generic_input(ReconConsts.PendingOrReconciliate);

            switch (input)
            {
                case "1":
                {
                    Set_path();
                    _file_loader.Create_pending_csvs(_path);
                }
                break;
                case "2":
                {
                    Decide_on_debug(); 
                }
                break;
            }
        }

        public void Decide_on_debug()
        {
            _input_output.Output_line("");
            _input_output.Output_options(new List<string>
            {
                $"1. Debug Mode A: Copy live sheet to debug version in [live location]/{ReconConsts.Backup_sub_folder}, and work on it from there.",
                $"2. Debug Mode B: Copy sheet from {ReconConsts.Source_debug_spreadsheet_path} to [live location]/{ReconConsts.Backup_sub_folder}, and work on it from there.",
                "3. Debug Mode C: Use fake spreadsheet repo (like you would get in .Net Core).",
                "4. Work in REAL mode"
            });

            string input = _input_output.Get_generic_input(ReconConsts.DebugOrReal);

            WorkingMode working_mode = WorkingMode.DebugA;
            switch (input)
            {
                case "1": { working_mode = WorkingMode.DebugA; Debug_mode_a(); } break;
                case "2": { working_mode = WorkingMode.DebugB; Debug_mode_b(); } break;
                case "3": { working_mode = WorkingMode.DebugC; Debug_mode_c(); } break;
                case "4": { working_mode = WorkingMode.Real; Real_mode(); } break;
            }

            Do_actual_reconciliation(working_mode);
        }

        public void Debug_mode_a()
        {
            Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Main_spreadsheet_path, ReconConsts.Main_spreadsheet_path);
            string debug_file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path, 
                ReconConsts.Backup_sub_folder, 
                ReconConsts.Debug_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(debug_file_path);
        }

        public void Debug_mode_b()
        {
            Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Source_debug_spreadsheet_path, ReconConsts.Main_spreadsheet_path);
            string debug_file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path,
                ReconConsts.Backup_sub_folder,
                ReconConsts.Debug_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(debug_file_path);
        }

        public void Debug_mode_c()
        {
            _spreadsheet_factory = new FakeSpreadsheetRepoFactory();
        }

        private void Real_mode()
        {
            Create_backup_of_real_spreadsheet(new Clock(), ReconConsts.Main_spreadsheet_path);
            string file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path,
                ReconConsts.Main_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(file_path);
        }

        private void Show_instructions(WorkingMode working_mode)
        {
            _input_output.Output_line("Here's how it works:");
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_01);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_02);
            _input_output.Output_line(ReconConsts.Instructions_line_03);
            _input_output.Output_line(ReconConsts.Instructions_line_04);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_05);
            _input_output.Output_line(ReconConsts.Instructions_line_06);
            _input_output.Output_line(ReconConsts.Instructions_line_07);
            _input_output.Output_line(ReconConsts.Instructions_line_08);
            _input_output.Output_line(ReconConsts.Instructions_line_09);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_10);
            _input_output.Output_line(ReconConsts.Instructions_line_11);
            _input_output.Output_line("");

            switch (working_mode)
            {
                case WorkingMode.DebugA: Show_debug_a_data_message(); break;
                case WorkingMode.DebugB: Show_debug_b_data_message(); break;
                case WorkingMode.DebugC: Show_debug_c_data_message(); break;
                case WorkingMode.Real: Show_real_data_message(); break;
            }
        }

        private void Show_debug_a_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE A!                     ***");
            _input_output.Output_line("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("*** It's an up to date copy of the main spreadsheet. It lives here: ***");
            _input_output.Output_line($"***  {ReconConsts.Main_spreadsheet_path}/{ReconConsts.Backup_sub_folder}  ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***  You can find debug versions of all csv files and a spreadsheet ***");
            _input_output.Output_line($"***     in [project root]/reconciliation-samples/For debugging     ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_debug_b_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE B!                     ***");
            _input_output.Output_line("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***      We'll copy the spreadsheet from the following folder:      ***");
            _input_output.Output_line($"***                {ReconConsts.Source_debug_spreadsheet_path}                 ***");
            _input_output.Output_line("***              The working copy will be placed here:              ***");
            _input_output.Output_line($"***  {ReconConsts.Main_spreadsheet_path}/{ReconConsts.Backup_sub_folder}  ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***  You can find debug versions of all csv files and a spreadsheet ***");
            _input_output.Output_line($"***     in [project root]/reconciliation-samples/For debugging     ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_debug_c_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE C!                     ***");
            _input_output.Output_line("***                  NO SPREADSHEET WILL BE USED!                   ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                 Using fake spreadsheet factory.                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_real_data_message()
        {
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("***                            WORKING IN REAL MODE!                            ***");
            _input_output.Output_line("***                      REAL SPREADSHEET WILL BE UPDATED!                      ***");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("*** (unless you're in .Net Core, in which case you're in debug mode by default) ***");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("");
        }

        private ReconciliationType Get_reconciliaton_type_from_user()
        {
            ReconciliationType result = ReconciliationType.Unknown;

            _input_output.Output_line("");
            _input_output.Output_line("What type are your third party and owned files?");
            _input_output.Output_options(new List<string>
            {
                ReconConsts.Accounting_type_01,
                ReconConsts.Accounting_type_02,
                ReconConsts.Accounting_type_03,
                ReconConsts.Accounting_type_04,
            });

            string input = _input_output.Get_generic_input(ReconConsts.Four_accounting_types);

            switch (input)
            {
                case "1": result = ReconciliationType.CredCard1AndCredCard1InOut; break;
                case "2": result = ReconciliationType.CredCard2AndCredCard2InOut; break;
                case "3": result = ReconciliationType.BankAndBankIn; break;
                case "4": result = ReconciliationType.BankAndBankOut; break;
            }

            return result;
        }

        private void Set_path_and_file_names()
        {
            _input_output.Output_line("Mathematical dude! Let's do some reconciliating. Type Exit at any time to leave (although to be honest I'm not sure that actually works...)");

            bool using_defaults = Set_all_file_details();

            if (!using_defaults)
            {
                Set_path();
                Set_third_party_file_name();
                Set_owned_file_name();
            }
        }

        private void Set_path()
        {
            _input_output.Output_line("");
            _input_output.Output_line("Would you like to enter a file path or use the default?");
            _input_output.Output_options(new List<string>
            {
                "1. Enter a path",
                $"2. Use default ({ReconConsts.Default_file_path})"
            });

            string input = _input_output.Get_generic_input(ReconConsts.PathOrDefault);

            switch (input)
            {
                case "1": _path = _input_output.Get_input(ReconConsts.EnterCsvPath); break;
                case "2": _path = ReconConsts.Default_file_path; break;
            }
        }

        private void Set_third_party_file_name()
        {
            _input_output.Output_line("");
            _input_output.Output_line("Would you like to enter a file name for your third party csv file, or use a default?");
            _input_output.Output_options(new List<string>
            {
                "1. Enter a file name",
                string.Format(ReconConsts.File_name_option_02, ReconConsts.Default_cred_card1_file_name),
                string.Format(ReconConsts.File_name_option_03, ReconConsts.Default_cred_card2_file_name),
                string.Format(ReconConsts.File_name_option_04, ReconConsts.Default_bank_file_name)
            });

            string input = _input_output.Get_generic_input(ReconConsts.Four_file_name_options);

            switch (input)
            {
                case "1": _third_party_file_name = _input_output.Get_input(ReconConsts.EnterThirdPartyFileName); break;
                case "2": _third_party_file_name = ReconConsts.Default_cred_card1_file_name; break;
                case "3": _third_party_file_name = ReconConsts.Default_cred_card2_file_name; break;
                case "4": _third_party_file_name = ReconConsts.Default_bank_file_name; break;
            }
        }

        private bool Set_all_file_details()
        {
            bool success = true;

            _input_output.Output_line("");
            _input_output.Output_line("Would you like to enter your own file details, or use defaults?");
            _input_output.Output_options(new List<string>
            {
                "1. Enter my own file details",
                ReconConsts.File_details_02,
                ReconConsts.File_details_03,
                ReconConsts.File_details_04,
                ReconConsts.File_details_05,
            });

            string input = _input_output.Get_generic_input(ReconConsts.Five_file_details);

            success = Set_file_details_according_to_user_input(input);

            return success;
        }

        private void Set_owned_file_name()
        {
            _input_output.Output_line("");
            _input_output.Output_line("Would you like to enter a file name for your own csv file, or use a default?");
            _input_output.Output_options(new List<string>
            {
                "1. Enter a file name",
                string.Format(ReconConsts.File_option_02, ReconConsts.Default_cred_card1_in_out_file_name),
                string.Format(ReconConsts.File_option_03, ReconConsts.Default_cred_card2_in_out_file_name),
                string.Format(ReconConsts.File_option_04, ReconConsts.DefaultBankInFileName),
                string.Format(ReconConsts.File_option_05, ReconConsts.DefaultBankOutFileName),
            });

            string input = _input_output.Get_generic_input(ReconConsts.Five_file_options);

            Capture_owned_file_name(input);
        }

        private void Capture_owned_file_name(string input)
        {
            switch (input)
            {
                case "1":
                {
                    _owned_file_name = _input_output.Get_input(ReconConsts.EnterOwnedFileName);
                }
                    break;
                case "2":
                {
                    _owned_file_name = ReconConsts.Default_cred_card1_in_out_file_name;
                }
                    break;
                case "3":
                {
                    _owned_file_name = ReconConsts.Default_cred_card2_in_out_file_name;
                }
                    break;
                case "4":
                {
                    _owned_file_name = ReconConsts.DefaultBankInFileName;
                }
                    break;
                case "5":
                {
                    _owned_file_name = ReconConsts.DefaultBankOutFileName;
                }
                    break;
            }
        }

        public bool Set_file_details_according_to_user_input(string input)
        {
            bool success = true;
            _reconciliation_type = ReconciliationType.Unknown;

            switch (input)
            {
                case "1":
                    {
                        success = false;
                        _reconciliation_type = Get_reconciliaton_type_from_user();
                    }
                    break;
                case "2":
                    {
                        _owned_file_name = ReconConsts.Default_cred_card1_in_out_file_name;
                        _third_party_file_name = ReconConsts.Default_cred_card1_file_name;
                        _reconciliation_type = ReconciliationType.CredCard1AndCredCard1InOut;
                    }
                    break;
                case "3":
                    {
                        _owned_file_name = ReconConsts.Default_cred_card2_in_out_file_name;
                        _third_party_file_name = ReconConsts.Default_cred_card2_file_name;
                        _reconciliation_type = ReconciliationType.CredCard2AndCredCard2InOut;
                    }
                    break;
                case "4":
                    {
                        _owned_file_name = ReconConsts.DefaultBankInFileName;
                        _third_party_file_name = ReconConsts.Default_bank_file_name;
                        _reconciliation_type = ReconciliationType.BankAndBankIn;
                    }
                    break;
                case "5":
                    {
                        _owned_file_name = ReconConsts.DefaultBankOutFileName;
                        _third_party_file_name = ReconConsts.Default_bank_file_name;
                        _reconciliation_type = ReconciliationType.BankAndBankOut;
                    }
                    break;
            }

            if (success)
            {
                _path = ReconConsts.Default_file_path;
                _input_output.Output_line("You are using the following defaults:");
                _input_output.Output_line("File path will be " + _path);
                _input_output.Output_line("Third party file name will be " + _third_party_file_name + ".csv");
                _input_output.Output_line("Owned file name will be " + _owned_file_name + ".csv");
            }

            return success;
        }

        public void Do_actual_reconciliation(WorkingMode working_mode)
        {
            try
            {
                Show_instructions(working_mode);
                Set_path_and_file_names();
                Do_matching();
            }
            catch (Exception exception)
            {
                if (exception.Message.ToUpper() == "EXIT")
                {
                    _input_output.Output_line("Taking you back to the main screen so you can start again if you want.");
                }
                else
                {
                    _input_output.Show_error(exception);
                }
            }
        }

        private void Do_matching()
        {
            var main_file_paths = new FilePaths
            {
                Main_path = _path,
                Third_party_file_name = _third_party_file_name,
                Owned_file_name = _owned_file_name
            };

            var reconciliation_interface = _file_loader.Load_specific_files_for_reconciliation_type(main_file_paths, _reconciliation_type);
            reconciliation_interface?.Do_the_matching();
        }

        #endregion User Instructions and Input

        #region Debug Spreadsheet Operations

        public void Copy_source_spreadsheet_to_debug_spreadsheet(string source_spreadsheet_path, string main_spreadsheet_path)
        {
            string source_file_path = Path.Combine(source_spreadsheet_path, ReconConsts.Main_spreadsheet_file_name);
            if (File.Exists(source_file_path))
            {
                string debug_file_path = Path.Combine(
                    main_spreadsheet_path, 
                    ReconConsts.Backup_sub_folder,
                    ReconConsts.Debug_spreadsheet_file_name);
                File.Copy(source_file_path, debug_file_path, true);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        public void Create_backup_of_real_spreadsheet(IClock clock, string spreadsheet_path)
        {
            string source_file_path = Path.Combine(spreadsheet_path, ReconConsts.Main_spreadsheet_file_name);
            if (File.Exists(source_file_path))
            {
                string file_name_prefix = $"{ReconConsts.Backup_sub_folder}\\real_backup_";
                file_name_prefix = file_name_prefix + clock.Now_date_time();
                file_name_prefix = file_name_prefix.Replace(" ", "_").Replace(":", "-").Replace("/", "-");
                string backup_file_name = file_name_prefix + "_" + ReconConsts.Main_spreadsheet_file_name;
                string backup_file_path = spreadsheet_path + "\\" + backup_file_name;

                File.Copy(source_file_path, backup_file_path, true);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        public void Inject_spreadsheet_factory(ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _spreadsheet_factory = spreadsheet_factory;
        }

        #endregion Debug Spreadsheet Operations

        #region Get budgeting months

        public BudgetingMonths Recursively_ask_for_budgeting_months(ISpreadsheet spreadsheet)
        {
            DateTime next_unplanned_month = Get_next_unplanned_month(spreadsheet);
            int last_month_for_budget_planning = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month.Month);
            var budgeting_months = new BudgetingMonths
            {
                Next_unplanned_month = next_unplanned_month.Month,
                Last_month_for_budget_planning = last_month_for_budget_planning,
                Start_year = next_unplanned_month.Year
            };
            if (last_month_for_budget_planning != 0)
            {
                budgeting_months.Last_month_for_budget_planning = Confirm_budgeting_month_choices_with_user(budgeting_months, spreadsheet);
            }
            return budgeting_months;
        }

        private DateTime Get_next_unplanned_month(ISpreadsheet spreadsheet)
        {
            DateTime default_month = DateTime.Today;
            DateTime next_unplanned_month = default_month;
            bool bad_input = false;
            try
            {
                next_unplanned_month = spreadsheet.Get_next_unplanned_month();
            }
            catch (Exception)
            {
                string new_month = _input_output.Get_input(ReconConsts.CantFindMortgageRow);
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
                _input_output.Output_line(ReconConsts.DefaultUnplannedMonth);
                next_unplanned_month = default_month;
            }

            return next_unplanned_month;
        }

        private int Get_last_month_for_budget_planning(ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            string next_unplanned_month_as_string = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(next_unplanned_month);
            var request_to_enter_month = String.Format(ReconConsts.EnterMonths, next_unplanned_month_as_string);
            string month = _input_output.Get_input(request_to_enter_month);
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

            result = Handle_zero_month_choice_result(result, spreadsheet, next_unplanned_month);
            return result;
        }

        private int Confirm_budgeting_month_choices_with_user(BudgetingMonths budgeting_months, ISpreadsheet spreadsheet)
        {
            var new_result = budgeting_months.Last_month_for_budget_planning;
            string input = Get_response_to_budgeting_months_confirmation_message(budgeting_months);

            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                // I know this doesn't really do anything but I found the if statement easier to parse this way round.
                new_result = budgeting_months.Last_month_for_budget_planning;
            }
            else
            {
                // Recursion ftw!
                new_result = Get_last_month_for_budget_planning(spreadsheet, budgeting_months.Next_unplanned_month);
            }

            return new_result;
        }

        private string Get_response_to_budgeting_months_confirmation_message(BudgetingMonths budgeting_months)
        {
            string first_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Next_unplanned_month);
            string second_month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(budgeting_months.Last_month_for_budget_planning);

            int month_span = budgeting_months.Num_budgeting_months();

            var confirmation_text = String.Format(ReconConsts.ConfirmMonthInterval, first_month, second_month, month_span);

            return _input_output.Get_input(confirmation_text);
        }

        private int Handle_zero_month_choice_result(int chosen_month, ISpreadsheet spreadsheet, int next_unplanned_month)
        {
            var new_result = chosen_month;
            if (chosen_month == 0)
            {
                var input = _input_output.Get_input(ReconConsts.ConfirmBadMonth);

                if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
                {
                    new_result = 0;
                    _input_output.Output_line(ReconConsts.ConfirmNoMonthlyBudgeting);
                }
                else
                {
                    // Recursion ftw!
                    new_result = Get_last_month_for_budget_planning(spreadsheet, next_unplanned_month);
                }
            }
            return new_result;
        }

        #endregion Get budgeting months
    }
}
