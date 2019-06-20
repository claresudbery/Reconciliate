using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Loaders;
using ConsoleCatchall.Console.Reconciliation.Reconciliators;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public class BankAndBankInLoaderTests
    {
        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_Owned_file_descriptor_from_loading_info()
        {
            // Arrange
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object, new Mock<ISpreadsheetRepoFactory>().Object);
            var loading_info = BankAndBankInData.LoadingInfo;
            int start_divider_row = 1;
            int end_divider_row = 4;
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    loading_info.Monthly_budget_data.Sheet_name,
                    loading_info.Monthly_budget_data.Start_divider,
                    2))
                .Returns(start_divider_row);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    loading_info.Monthly_budget_data.Sheet_name,
                    loading_info.Monthly_budget_data.End_divider,
                    2))
                .Returns(end_divider_row);
            mock_spreadsheet_repo.Setup(x => x.Get_rows_as_records<BankRecord>(
                    loading_info.Monthly_budget_data.Sheet_name,
                    start_divider_row + 1,
                    end_divider_row - 1,
                    loading_info.Monthly_budget_data.First_column_number,
                    loading_info.Monthly_budget_data.Last_column_number))
                .Returns(new List<BankRecord>());
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);

            // Act
            var reconciliation_interface = bank_and_bank_in_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths);

            // Assert 
            Assert.AreEqual(loading_info.Owned_file_descriptor, reconciliation_interface.Owned_file_descriptor);
        }

        [Test]
        public void Bank_and_bank_in__Merge_bespoke_data_with_pending_file__Will_merge_unreconciled_employer_expenses_with_pending_file()
        {
            // Arrange
            var mock_input_output = new Mock<IInputOutput>();
            var mock_spreadsheet_repo = new Mock<ISpreadsheetRepo>();
            var divider_row_number = 10;
            var last_row_number = 11;
            var mock_cell_row = new Mock<ICellRow>();
            var description = "Hello";
            var code = Codes.Expenses;
            mock_cell_row.Setup(x => x.Read_cell(ExpectedIncomeRecord.DescriptionIndex)).Returns(description);
            mock_cell_row.Setup(x => x.Read_cell(ExpectedIncomeRecord.CodeIndex)).Returns(code);
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(MainSheetNames.Expected_in, Dividers.Divider_text, 2)).Returns(divider_row_number);
            mock_spreadsheet_repo.Setup(x => x.Last_row_number(MainSheetNames.Expected_in)).Returns(last_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(MainSheetNames.Expected_in, last_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var bank_and_bank_in_loader = new BankAndBankInLoader(mock_input_output.Object, new Mock<ISpreadsheetRepoFactory>().Object);

            // Act
            bank_and_bank_in_loader.Merge_bespoke_data_with_pending_file(
                mock_input_output.Object,
                spreadsheet,
                mock_pending_file.Object,
                new BudgetingMonths(),
                BankAndBankInData.LoadingInfo);

            // Assert
            mock_input_output.Verify(x => x.Output_line(ReconConsts.Loading_expenses));
            Assert.AreEqual(1, pending_records.Count);
            Assert.AreEqual(description, pending_records[0].Description);
        }
    }
}