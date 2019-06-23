using System.Collections.Generic;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Loaders;
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
        public void Load_files_and_merge_data_will_not_load_data_when_testing()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var budgeting_months = new BudgetingMonths();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);
            var exception_thrown = false;
            loading_info.File_paths.Main_path = "This is not a path";

            // Act
            try
            {
                bank_and_bank_in_loader.Load(
                    mock_spreadsheet.Object,
                    budgeting_months,
                    loading_info.File_paths,
                    new Mock<IFileIO<BankRecord>>().Object,
                    mock_pending_file.Object,
                    mock_third_party_file_io.Object,
                    mock_owned_file_io.Object,
                    loading_info);
            }
            catch (DirectoryNotFoundException)
            {
                exception_thrown = true;

                // Clean up
                loading_info.File_paths.Main_path = ReconConsts.Default_file_path;
            }

            // Assert
            Assert.IsFalse(exception_thrown);
        }

        [Test]
        public void Will_not_delete_unreconciled_rows_when_merging_pending_with_unreconciled()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var budgeting_months = new BudgetingMonths();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);

            // Act
            bank_and_bank_in_loader.Load(
                mock_spreadsheet.Object,
                budgeting_months,
                loading_info.File_paths,
                new Mock<IFileIO<BankRecord>>().Object,
                mock_pending_file.Object,
                mock_third_party_file_io.Object,
                mock_owned_file_io.Object,
                loading_info);

            // Assert
            mock_spreadsheet
                .Verify(x => x.Delete_unreconciled_rows(It.IsAny<string>()),
                    Times.Never);
        }

        [Test]
        public void Load__Will_fetch_and_generate_data_from_spreadsheet()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var budgeting_months = new BudgetingMonths();
            var mock_spreadsheet = new Mock<ISpreadsheet>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);

            // Act
            bank_and_bank_in_loader.Load(
                mock_spreadsheet.Object,
                budgeting_months,
                loading_info.File_paths,
                new Mock<IFileIO<BankRecord>>().Object,
                mock_pending_file.Object,
                mock_third_party_file_io.Object,
                mock_owned_file_io.Object,
                loading_info);

            // Assert 
            mock_spreadsheet.Verify(x => x.Add_budgeted_monthly_data_to_pending_file(
                budgeting_months, 
                mock_pending_file.Object, 
                loading_info.Monthly_budget_data));
            mock_spreadsheet.Verify(x => x.Add_unreconciled_rows_to_csv_file(loading_info.Sheet_name, mock_pending_file.Object));
        }

        [Test]
        public void Load__Will_load_and_write_to_pending_file()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);

            // Act
            bank_and_bank_in_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths,
                new Mock<IFileIO<BankRecord>>().Object,
                mock_pending_file.Object,
                mock_third_party_file_io.Object,
                mock_owned_file_io.Object,
                loading_info);

            // Assert 
            mock_pending_file.Verify(x => x.Load(true, loading_info.Default_separator, true));
            mock_pending_file.Verify(x => x.Convert_source_line_separators(loading_info.Default_separator, loading_info.Loading_separator));
            mock_pending_file.Verify(x => x.Update_source_lines_for_output(loading_info.Loading_separator));
            mock_pending_file.Verify(x => x.Write_to_file_as_source_lines(loading_info.File_paths.Owned_file_name));
        }

        [Test]
        public void Load__Will_set_file_paths_on_all_file_io_objects()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file_io = new Mock<IFileIO<BankRecord>>();
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);

            // Act
            bank_and_bank_in_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths,
                mock_pending_file_io.Object,
                mock_pending_file.Object,
                mock_third_party_file_io.Object,
                mock_owned_file_io.Object,
                loading_info);

            // Assert 
            mock_pending_file_io.Verify(x => x.Set_file_paths(loading_info.File_paths.Main_path, loading_info.Pending_file_name));
            mock_third_party_file_io.Verify(x => x.Set_file_paths(loading_info.File_paths.Main_path, loading_info.File_paths.Third_party_file_name));
            mock_owned_file_io.Verify(x => x.Set_file_paths(loading_info.File_paths.Main_path, loading_info.File_paths.Owned_file_name));
        }

        [Test]
        public void Load__Will_create_a_reconciliation_interface_using_file_details_from_loading_info()
        {
            // Arrange
            var loading_info = BankAndBankInData.LoadingInfo;
            var mock_spreadsheet_repo = FileLoaderTestHelper.Create_mock_spreadsheet_for_loading<BankRecord>(loading_info);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            mock_pending_file.Setup(x => x.Records).Returns(new List<BankRecord>());
            var mock_third_party_file_io = new Mock<IFileIO<ActualBankRecord>>();
            mock_third_party_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<ActualBankRecord>());
            var mock_owned_file_io = new Mock<IFileIO<BankRecord>>();
            mock_owned_file_io.Setup(x => x.Load(It.IsAny<List<string>>(), null)).Returns(new List<BankRecord>());
            var bank_and_bank_in_loader = new BankAndBankInLoader(new Mock<IInputOutput>().Object);

            // Act
            var reconciliation_interface = bank_and_bank_in_loader.Load(
                spreadsheet,
                new BudgetingMonths(),
                loading_info.File_paths,
                new Mock<IFileIO<BankRecord>>().Object,
                mock_pending_file.Object,
                mock_third_party_file_io.Object,
                mock_owned_file_io.Object,
                loading_info);

            // Assert 
            Assert.AreEqual(loading_info.Third_party_descriptor, reconciliation_interface.Third_party_descriptor);
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
            mock_spreadsheet_repo.Setup(x => x.Find_row_number_of_last_row_containing_cell(
                    MainSheetNames.Expected_in, 
                    Dividers.Divider_text, 
                    SpreadsheetConsts.DefaultDividerColumn))
                .Returns(divider_row_number);
            mock_spreadsheet_repo.Setup(x => x.Last_row_number(MainSheetNames.Expected_in)).Returns(last_row_number);
            mock_spreadsheet_repo.Setup(x => x.Read_specified_row(MainSheetNames.Expected_in, last_row_number)).Returns(mock_cell_row.Object);
            var spreadsheet = new Spreadsheet(mock_spreadsheet_repo.Object);
            var mock_pending_file = new Mock<ICSVFile<BankRecord>>();
            var pending_records = new List<BankRecord>();
            mock_pending_file.Setup(x => x.Records).Returns(pending_records);
            var bank_and_bank_in_loader = new BankAndBankInLoader(mock_input_output.Object);

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