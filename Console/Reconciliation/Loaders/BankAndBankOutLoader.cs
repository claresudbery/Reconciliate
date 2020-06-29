using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsoleCatchall.Console.Reconciliation.Extensions;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class BankAndBankOutLoader : ILoader<ActualBankRecord, BankRecord>
    {
        public DataLoadingInformation<ActualBankRecord, BankRecord> Loading_info()
        {
            return new DataLoadingInformation<ActualBankRecord, BankRecord>
            {
                File_paths = new FilePaths
                {
                    Main_path = ReconConsts.Default_file_path,
                    Third_party_file_name = ReconConsts.Default_bank_file_name,
                    Owned_file_name = ReconConsts.DefaultBankOutFileName
                },
                Default_separator = ',',
                Loading_separator = '^',
                Pending_file_name = ReconConsts.DefaultBankOutPendingFileName,
                Sheet_name = MainSheetNames.Bank_out,
                Third_party_descriptor = ReconConsts.Bank_descriptor,
                Owned_file_descriptor = ReconConsts.BankOutDescriptor,
                Loader = this,
                Monthly_budget_data = new BudgetItemListData
                {
                    Budget_sheet_name = MainSheetNames.Budget_out,
                    Owned_sheet_name = MainSheetNames.Bank_out,
                    Start_divider = Dividers.Sodds,
                    End_divider = Dividers.Cred_card1,
                    First_column_number = 2,
                    Last_column_number = 6,
                    Third_party_desc_col = 15
                },
                Annual_budget_data = new BudgetItemListData
                {
                    Budget_sheet_name = MainSheetNames.Budget_out,
                    Owned_sheet_name = MainSheetNames.Bank_out,
                    Start_divider = Dividers.Annual_sodds,
                    End_divider = Dividers.Annual_total,
                    First_column_number = 2,
                    Last_column_number = 6,
                    Third_party_desc_col = 15
                }
            };
        }

        public IDataFile<ActualBankRecord> Create_new_third_party_file(IFileIO<ActualBankRecord> third_party_file_io)
        {
            var csv_file = new CSVFile<ActualBankRecord>(third_party_file_io);
            return new ActualBankOutFile(csv_file);
        }

        public IDataFile<BankRecord> Create_new_owned_file(IFileIO<BankRecord> owned_file_io)
        {
            var csv_file = new CSVFile<BankRecord>(owned_file_io);
            return new GenericFile<BankRecord>(csv_file);
        }

        public void Merge_bespoke_data_with_pending_file(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<ActualBankRecord, BankRecord> data_loading_info)
        {
            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                pending_file,
                ReconConsts.Cred_card1_name,
                ReconConsts.Cred_card1_dd_description);

            Add_most_recent_credit_card_direct_debits(
                input_output,
                spreadsheet,
                pending_file,
                ReconConsts.Cred_card2_name,
                ReconConsts.Cred_card2_dd_description);

            // Note that we can't update bank balance here because we don't have access to third party file,
            // so we do it below in Do_actions_which_require_third_party_data_access.
        }

        public void Generate_ad_hoc_data(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            BudgetingMonths budgeting_months,
            DataLoadingInformation<ActualBankRecord, BankRecord> data_loading_info)
        {
            if (budgeting_months.Do_expected_out_budgeting)
            {
                Update_owed_CHB(input_output, spreadsheet, budgeting_months);

                Update_weekly_item(input_output, spreadsheet, budgeting_months, "weekly spends", Codes.Code004, Codes.Code074);
                Update_weekly_item(input_output, spreadsheet, budgeting_months, "grocery shopping", Codes.Code005, Codes.Code075);

                Update_monthly_item(input_output, spreadsheet, budgeting_months, "yoga", Codes.Code078, Codes.Code078);
                Update_monthly_item(input_output, spreadsheet, budgeting_months, "fuel", Codes.Code006, Codes.Code006);
                Update_monthly_item(input_output, spreadsheet, budgeting_months, "vet", Codes.Code007, Codes.Code007);
                Update_monthly_item(input_output, spreadsheet, budgeting_months, "internet", Codes.Code011, Codes.Code011);
            }
        }

        private void Update_monthly_item(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            BudgetingMonths budgeting_months,
            string item, 
            string budget_code, 
            string expected_out_code)
        {
            var input = input_output.Get_input(Num_months_question(budgeting_months, item));
            var num_months = 0;
            if (int.TryParse(input, out num_months))
            {
                spreadsheet.Update_expected_out(num_months, budget_code, expected_out_code);
            }
        }

        private string Num_months_question(BudgetingMonths budgeting_months, string item)
        {
            var months = budgeting_months.Num_budgeting_months();
            var start = budgeting_months.Budgeting_start_date().ToString("MMM", CultureInfo.CurrentCulture);
            var end = budgeting_months.Budgeting_end_date().ToString("MMM", CultureInfo.CurrentCulture);
            return $"You're budgeting for {months} months, {start} til {end}. How many months of {item} do you want to budget for?";
        }

        private void Update_weekly_item(
            IInputOutput input_output, 
            ISpreadsheet spreadsheet, 
            BudgetingMonths budgeting_months,
            string item,
            string budget_code,
            string expected_out_code)
        {
            var week_getter = new WeekGetter(input_output, new Clock());
            var weeks = week_getter.Decide_num_weeks(item, budgeting_months);
            spreadsheet.Update_expected_out(weeks.NumWeeks, budget_code, expected_out_code);
        }

        private void Update_owed_CHB(IInputOutput input_output, ISpreadsheet spreadsheet, BudgetingMonths budgeting_months)
        {
            var input = input_output.Get_input("Do you want to add to the CHB total? The answer is probably No if you're not doing monthly transaction budgeting this time. Enter y/Y for Yes.");
            if (!String.IsNullOrEmpty(input) && input.ToUpper() == "Y")
            {
                spreadsheet.Update_owed_CHB(budgeting_months);
            }
        }

        private void Add_most_recent_credit_card_direct_debits(
            IInputOutput input_output,
            ISpreadsheet spreadsheet,
            ICSVFile<BankRecord> pending_file,
            string cred_card_name,
            string direct_debit_description)
        {
            var most_recent_cred_card_direct_debit = spreadsheet.Get_most_recent_row_containing_text<BankRecord>(
                MainSheetNames.Bank_out, 
                direct_debit_description,
                new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});

            var next_date = most_recent_cred_card_direct_debit.Date.AddMonths(1);
            var input = input_output.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                cred_card_name,
                next_date.ToShortDateString()));
            while (input != "0")
            {
                double amount;
                if (double.TryParse(input, out amount))
                {
                    pending_file.Records.Add(new BankRecord
                    {
                        Date = next_date,
                        Description = direct_debit_description,
                        Type = "POS",
                        Unreconciled_amount = amount
                    });
                }
                next_date = next_date.Date.AddMonths(1);
                input = input_output.Get_input(string.Format(
                    ReconConsts.AskForCredCardDirectDebit, 
                    cred_card_name,
                    next_date.ToShortDateString()));
            }
        }

        public void Do_actions_which_require_third_party_data_access(
            IDataFile<ActualBankRecord> third_party_file,
            IDataFile<BankRecord> owned_file,
            ISpreadsheet spreadsheet,
            IInputOutput input_output)
        {
            Update_bank_balance(
                (third_party_file as ActualBankOutFile),
                spreadsheet,
                input_output);
        }

        private void Update_bank_balance(
            ActualBankOutFile actual_bank_out_file,
            ISpreadsheet spreadsheet,
            IInputOutput input_output)
        {
            input_output.Output_line("Writing bank balance to spreadsheet...");

            IList<ActualBankRecord> potential_balance_rows = actual_bank_out_file.Get_potential_balance_rows().ToList();

            if (!potential_balance_rows.Any())
            {
                input_output.Output_line("");
                input_output.Get_generic_input(ReconConsts.CantFindBalanceRow);
            }
            else
            {
                ActualBankRecord balance_row = Choose_balance_row(potential_balance_rows, input_output);

                string balance_description = String.Format(
                    ReconConsts.BankBalanceDescription,
                    ReconConsts.Bank_descriptor,
                    balance_row.Description,
                    balance_row.Main_amount().To_csv_string(true),
                    balance_row.Date.ToString(@"dd\/MM\/yyyy"));

                spreadsheet.Update_balance_on_totals_sheet(
                    Codes.Bank_bal,
                    balance_row.Balance,
                    balance_description,
                    balance_column: ReconConsts.BankBalanceAmountColumn,
                    text_column: ReconConsts.BankBalanceTextColumn,
                    code_column: ReconConsts.BankBalanceCodeColumn,
                    input_output: input_output);
            }
        }

        private ActualBankRecord Choose_balance_row(IList<ActualBankRecord> potential_balance_rows, IInputOutput input_output)
        {
            var result = potential_balance_rows.First();

            if (potential_balance_rows.Count > 1)
            {
                input_output.Output_line("");
                input_output.Output_line(String.Format(ReconConsts.MultipleBalanceRows, result.To_string()));

                for (int index = 0; index < potential_balance_rows.Count; index++)
                {
                    input_output.Output_line($"{index + 1}. {potential_balance_rows[index].To_string()}");
                }

                string input = input_output.Get_generic_input(ReconConsts.MultipleBalanceRows);

                int new_index = 0;
                if (int.TryParse(input, out new_index))
                {
                    result = potential_balance_rows[new_index - 1];
                }
            }

            return result;
        }
    }
}