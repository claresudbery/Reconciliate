using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Matchers;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class PathSetter
    {
        private readonly IInputOutput _input_output;

        private string _path = "";
        private string _third_party_file_name = "";
        private string _owned_file_name = "";

        private IMatcher _matcher = null;
        private readonly ISpreadsheetRepoFactory _spreadsheet_factory;

        public PathSetter(IInputOutput input_output, ISpreadsheetRepoFactory spreadsheet_factory)
        {
            _input_output = input_output;
            _spreadsheet_factory = spreadsheet_factory;
        }

        public FilePaths Set_path_and_file_names()
        {
            _input_output.Output_line("Mathematical dude! Let's do some reconciliating. Type Exit at any time to leave (although to be honest I'm not sure that actually works...)");

            bool using_defaults = Set_all_file_details();

            if (!using_defaults)
            {
                Set_path();
                Set_third_party_file_name();
                Set_owned_file_name();
            }

            return new FilePaths
            {
                Matcher = _matcher,
                Main_path = _path,
                Third_party_file_name = _third_party_file_name,
                Owned_file_name = _owned_file_name
            };
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

        private bool Set_file_details_according_to_user_input(string input)
        {
            bool success = true;
            _matcher = null;

            switch (input)
            {
                case "1":
                    {
                        success = false;
                        _matcher = Get_reconciliaton_type_from_user();
                    }
                    break;
                case "2":
                    {
                        _owned_file_name = ReconConsts.Default_cred_card1_in_out_file_name;
                        _third_party_file_name = ReconConsts.Default_cred_card1_file_name;
                        _matcher = new CredCard1AndCredCard1InOutMatcher(_input_output, _spreadsheet_factory);
                    }
                    break;
                case "3":
                    {
                        _owned_file_name = ReconConsts.Default_cred_card2_in_out_file_name;
                        _third_party_file_name = ReconConsts.Default_cred_card2_file_name;
                        _matcher = new CredCard2AndCredCard2InOutMatcher(_input_output, _spreadsheet_factory);
                    }
                    break;
                case "4":
                    {
                        _owned_file_name = ReconConsts.DefaultBankInFileName;
                        _third_party_file_name = ReconConsts.Default_bank_file_name;
                        _matcher = new BankAndBankInMatcher(_input_output, _spreadsheet_factory, new BankAndBankInLoader(_spreadsheet_factory));
                    }
                    break;
                case "5":
                    {
                        _owned_file_name = ReconConsts.DefaultBankOutFileName;
                        _third_party_file_name = ReconConsts.Default_bank_file_name;
                        _matcher = new BankAndBankOutMatcher(_input_output, _spreadsheet_factory);
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

        private IMatcher Get_reconciliaton_type_from_user()
        {
            IMatcher result = null;

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
                case "1": result = new CredCard1AndCredCard1InOutMatcher(_input_output, _spreadsheet_factory); break;
                case "2": result = new CredCard2AndCredCard2InOutMatcher(_input_output, _spreadsheet_factory); break;
                case "3": result = new BankAndBankInMatcher(_input_output, _spreadsheet_factory, new BankAndBankInLoader(_spreadsheet_factory)); break;
                case "4": result = new BankAndBankOutMatcher(_input_output, _spreadsheet_factory); break;
            }

            return result;
        }

        public string Set_path()
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

            return _path;
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
    }
}