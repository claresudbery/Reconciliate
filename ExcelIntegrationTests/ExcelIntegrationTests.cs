using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Records;
using ExcelIntegrationTests.Records;
using ExcelIntegrationTests.TestUtils;
using ExcelLibrary;
using Interfaces;
using Interfaces.Constants;
using NUnit.Framework;

namespace ExcelIntegrationTests
{
    [TestFixture]
    public class ExcelIntegrationTests
    {
        private static string _testSpreadsheetFileNameAndPath;
        private static string _csvFilePath;
        private static ExcelSpreadsheetRepo _excelSpreadsheet;

        public ExcelIntegrationTests()
        {
            PopulateFilePaths();
            TestHelper.SetCorrectDateFormatting();
        }

        private void PopulateFilePaths()
        {
            string currentPath = TestContext.CurrentContext.TestDirectory;

            _testSpreadsheetFileNameAndPath =
                TestHelper.FullyQualifiedSpreadsheetFilePath(currentPath)
                + "/" + "Test-Spreadsheet.xlsx";

            _csvFilePath = TestHelper.FullyQualifiedCSVFilePath(currentPath);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _excelSpreadsheet.Dispose();
        }

        private void CloseAndReopenSpreadsheet()
        {
            _excelSpreadsheet.Dispose();
            _excelSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadLastRowOfSpecifiedWorksheetAsObjectList()
        {
            // Arrange
            CredCard1InOutRecord actualRowAsRecord = new CredCard1InOutRecord();
            List<String> expectedRow = new List<string>{
                "27/04/2018",
                "5.1",
                "",
                "pintipoplication",
                "10567.89"};

            // Act
            var actualRow = _excelSpreadsheet.ReadLastRow(TestSheetNames.CredCard);

            // Assert
            actualRowAsRecord.ReadFromSpreadsheetRow(actualRow);
            Assert.AreEqual(expectedRow[0], actualRowAsRecord.Date.ToShortDateString());
            Assert.AreEqual(expectedRow[1], actualRowAsRecord.UnreconciledAmount.ToString());
            Assert.AreEqual(expectedRow[3], actualRowAsRecord.Description);
            Assert.AreEqual(expectedRow[4], actualRowAsRecord.ReconciledAmount.ToString());
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadLastRowOfWorksheetAsCsv()
        {
            // Arrange
            var credCard1Record = new CredCard1InOutRecord();
            string expectedCsv = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";

            // Act
            var result = _excelSpreadsheet.ReadLastRowAsCsv(TestSheetNames.CredCard, credCard1Record);

            // Assert
            Assert.AreEqual(expectedCsv, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendAllRowsInCsvFileToSpecifiedWorksheet()
        {
            // Arrange
            var sheetName = TestSheetNames.Bank;
            var fileIO = new FileIO<BankRecord>(new ExcelSpreadsheetRepoFactory(""), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load();
            var initialLastRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);

            // Act
            _excelSpreadsheet.AppendCsvFile<BankRecord>(sheetName, csvFile);
            var newLastRow = _excelSpreadsheet.ReadLastRow(sheetName);
            var newRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);

            // Clean up
            for (int count = 1; count <= csvFile.Records.Count; count++)
            {
                _excelSpreadsheet.RemoveLastRow(sheetName);
            }

            // Assert
            var lastRecordInOrderedCsvFile = csvFile.RecordsOrderedForSpreadsheet()[csvFile.Records.Count - 1];
            Assert.AreEqual(lastRecordInOrderedCsvFile.Date, DateTime.FromOADate((double)newLastRow.ReadCell(0)));
            Assert.AreEqual(lastRecordInOrderedCsvFile.UnreconciledAmount, (Double)newLastRow.ReadCell(1));
            Assert.AreEqual(lastRecordInOrderedCsvFile.Type, (String)newLastRow.ReadCell(3));
            Assert.AreEqual(lastRecordInOrderedCsvFile.Description, (String)newLastRow.ReadCell(4));
            Assert.AreEqual(null, newLastRow.ReadCell(5));
            Assert.AreEqual(null, newLastRow.ReadCell(6));
            Assert.AreEqual(initialLastRowNumber + csvFile.Records.Count, newRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfLastRowContainingCell()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(TestSheetNames.CredCard, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfLastRowContainingCellDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.BadDivider;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(
                    worksheetName,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains("divider"));
            Assert.IsFalse(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoRowContainingCell()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.TestRecord;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowContainingCell(
                    worksheetName,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains("divider"));
            Assert.IsTrue(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfLastRowWithCellContainingText()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                TestSheetNames.CredCard,
                "Thingummybob", 
                new List<int>{2, 4, 6});

            // Assert
            Assert.AreEqual(6, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfLastRowWithCellContainingTextDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.BadDivider;
            const string targetText = "Thingummybob";

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                    worksheetName,
                    targetText,
                    new List<int> {ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn});
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(targetText));
            Assert.IsFalse(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoRowWithCellContainingText()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.CredCard;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfLastRowWithCellContainingText(
                    worksheetName,
                    ReconConsts.CredCard2DdDescription,
                    new List<int> {ReconConsts.DdDescriptionColumn});
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(ReconConsts.CredCard2DdDescription));
            Assert.IsTrue(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindRowNumberOfFirstRowContainingCell()
        {
            // Act
            var result = _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(TestSheetNames.CredCard, "pintipoplication", 4);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfFirstRowContainingCellDoesNotHaveCellInExpectedColumn()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.BadDivider;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(
                    worksheetName,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains("divider"));
            Assert.IsFalse(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowExceptionIfThereIsNoFirstRowContainingCell()
        {
            // Arrange
            bool exceptionThrown = false;
            String errorMessage = String.Empty;
            String worksheetName = TestSheetNames.TestRecord;

            // Act
            try
            {
                _excelSpreadsheet.FindRowNumberOfFirstRowContainingCell(
                    worksheetName,
                    Dividers.DividerText);
            }
            catch (Exception e)
            {
                exceptionThrown = true;
                errorMessage = e.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains("divider"));
            Assert.IsTrue(errorMessage.Contains("row"));
            Assert.IsTrue(errorMessage.Contains(worksheetName));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void ThrowsExceptionWhenAddingRowToSpreadsheetAlreadyOpen()
        {
            // Arrange
            var secondSpreadsheet = new ExcelSpreadsheetRepo(_testSpreadsheetFileNameAndPath);
            bool exceptionThrown = false;
            TestCsvRecord appendedRow = new TestCsvRecord().Build();

            // Act
            try
            {
                secondSpreadsheet.AppendCsvRecord(TestSheetNames.TestRecord, appendedRow);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.AreEqual(true, exceptionThrown);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendCsvRecordToSpecifiedWorksheet()
        {
            // Arrange
            var sheetName = TestSheetNames.CredCard1;
            var credCard1InOutRecord = new CredCard1InOutRecord
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                UnreconciledAmount = 1234.56,
                Matched = true,
                Description = "description",
                ReconciledAmount = 356.29
            };
            var initialLastRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);

            // Act
            _excelSpreadsheet.AppendCsvRecord(sheetName, credCard1InOutRecord);
            var newRow = _excelSpreadsheet.ReadLastRow(sheetName);
            var newRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);

            // Clean up
            _excelSpreadsheet.RemoveLastRow(sheetName);

            // Assert
            Assert.AreEqual(credCard1InOutRecord.Date, DateTime.FromOADate((double)newRow.ReadCell(CredCard1InOutRecord.DateIndex)));
            Assert.AreEqual(credCard1InOutRecord.UnreconciledAmount, (Double)newRow.ReadCell(CredCard1InOutRecord.UnreconciledAmountIndex));
            Assert.AreEqual("x", (String)newRow.ReadCell(CredCard1InOutRecord.MatchedIndex));
            Assert.AreEqual(credCard1InOutRecord.Description, (String)newRow.ReadCell(CredCard1InOutRecord.DescriptionIndex));
            Assert.AreEqual(credCard1InOutRecord.ReconciledAmount, (Double)newRow.ReadCell(CredCard1InOutRecord.ReconciledAmountIndex));
            Assert.AreEqual(initialLastRowNumber + 1, newRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillAppendRowsToWorksheetOrderedByMatchedAndThenByDateAndIncludingDivider()
        {
            // Arrange
            var sheetName = TestSheetNames.Bank;
            const bool doNotOrderByDate = false;
            var fileIO = new FileIO<BankRecord>(new ExcelSpreadsheetRepoFactory(""), _csvFilePath, "BankIn-formatted-date-only");
            var csvFile = new CSVFile<BankRecord>(fileIO);
            csvFile.Load(
                true,
                null,
                doNotOrderByDate);
            var initialLastRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);
            csvFile.Records[0].Matched = true;
            csvFile.Records[1].Matched = true;
            csvFile.Records[3].Matched = true;

            // Act
            _excelSpreadsheet.AppendCsvFile<BankRecord>(sheetName, csvFile);
            List<ICellRow> newRows = new List<ICellRow>();
            for (int newRowCount = 1; newRowCount <= csvFile.Records.Count + 1; newRowCount++)
            {
                newRows.Add(_excelSpreadsheet.ReadSpecifiedRow(sheetName, initialLastRowNumber + newRowCount));
            }

            // Clean up
            for (int count = 1; count <= csvFile.Records.Count + 1; count++)
            {
                _excelSpreadsheet.RemoveLastRow(sheetName);
            }

            // Assert
            Assert.AreEqual("ZZZThing2", (String)newRows[0].ReadCell(4));
            Assert.AreEqual("ZZZSpecialDescription001", (String)newRows[1].ReadCell(4));
            Assert.AreEqual("ZZZThing1", (String)newRows[2].ReadCell(4));
            Assert.AreEqual("Divider", (String)newRows[3].ReadCell(1));
            Assert.AreEqual("ZZZThing3", (String)newRows[4].ReadCell(4));
            Assert.AreEqual("ZZZSpecialDescription005", (String)newRows[5].ReadCell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanDeleteSpecifiedRows()
        {
            // Arrange
            var sheetName = TestSheetNames.CredCard;
            var testRecord = new CredCard1InOutRecord();
            string csvLine = String.Format("19/12/2016^£12.34^^Bantams^£33.44^");
            testRecord.Load(csvLine);
            var lastRow = new CredCard1InOutRecord();
            string expectedLastRow = "27/04/2018,£5.10,,\"pintipoplication\",\"£10,567.89\",";
            var initialLastRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);
            _excelSpreadsheet.AppendCsvRecord(sheetName, testRecord);
            _excelSpreadsheet.AppendCsvRecord(sheetName, testRecord);

            // Act
            _excelSpreadsheet.DeleteSpecifiedRows(sheetName, 12, 13);

            // Assert
            var newLastRow = _excelSpreadsheet.ReadLastRowAsCsv(sheetName, lastRow);
            var newLastRowNumber = _excelSpreadsheet.LastRowNumber(sheetName);
            Assert.AreEqual(newLastRowNumber, initialLastRowNumber);
            Assert.AreEqual(expectedLastRow, newLastRow);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void IfAmountCodeIsNotPresentThenSensibleErrorWillBeThrown()
        {
            // Arrange
            bool exceptionThrown = false;
            string nonExistentCode = "Non-Existent Code";
            string actualError = string.Empty;
            string sheetName = TestSheetNames.ExpectedOut;
            string expectedError = string.Format(ReconConsts.MissingCodeInWorksheet, nonExistentCode, sheetName);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheetName, nonExistentCode, 10);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                actualError = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown, "Exception should be thrown");
            Assert.AreEqual(expectedError, actualError);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void IfAmountCodeExistsInWrongColumnThenSensibleErrorWillBeThrown()
        {
            // Arrange
            bool exceptionThrown = false;
            string wrongColumnCode = "CodeInWrongColumn";
            string actualError = string.Empty;
            string sheetName = TestSheetNames.ExpectedOut;
            string expectedError = string.Format(ReconConsts.CodeInWrongPlace, wrongColumnCode, sheetName);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheetName, wrongColumnCode, 10);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                actualError = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown, "Exception should be thrown");
            Assert.AreEqual(expectedError, actualError);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadTextIdentifiedByRowAndColumn()
        {
            // Arrange
            string expectedText = "description1";
            int rowNumber = 4;
            int columnNumber = 3;

            // Act
            string actualText = _excelSpreadsheet.GetText(TestSheetNames.CredCard1, rowNumber, columnNumber);

            // Assert
            Assert.AreEqual(expectedText, actualText);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfTextIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheetName = TestSheetNames.CredCard1;
            int rowNumber = 50;
            int columnNumber = 3;
            bool exceptionThrown = false;
            string errorMessage = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetText(sheetName, rowNumber, columnNumber);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(errorMessage.Contains(sheetName), $"error message should contain {sheetName}");
            Assert.IsTrue(errorMessage.Contains($"{rowNumber}"), "error message should contain rowNumber");
            Assert.IsTrue(errorMessage.Contains($"{columnNumber}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadDateIdentifiedByRowAndColumn()
        {
            // Arrange
            DateTime expectedDate = new DateTime(2018, 3, 18);
            int rowNumber = 8;
            int columnNumber = 1;

            // Act
            DateTime actualDate = _excelSpreadsheet.GetDate(TestSheetNames.TestRecord, rowNumber, columnNumber);

            // Assert
            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfDateIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheetName = TestSheetNames.TestRecord;
            int rowNumber = 50;
            int columnNumber = 1;
            bool exceptionThrown = false;
            string errorMessage = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetDate(sheetName, rowNumber, columnNumber);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(errorMessage.Contains(sheetName), $"error message should contain {sheetName}");
            Assert.IsTrue(errorMessage.Contains($"{rowNumber}"), "error message should contain rowNumber");
            Assert.IsTrue(errorMessage.Contains($"{columnNumber}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadAmountIdentifiedByRowAndColumn()
        {
            // Arrange
            double expectedAmount = 37957.90;
            int rowNumber = 5;
            int columnNumber = 4;

            // Act
            double actualAmount = _excelSpreadsheet.GetAmount(TestSheetNames.CredCard1, rowNumber, columnNumber);

            // Assert
            Assert.AreEqual(expectedAmount, actualAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfAmountIdentifiedByRowAndColumnIsNotPresent()
        {
            // Arrange
            string sheetName = TestSheetNames.CredCard1;
            int rowNumber = 50;
            int columnNumber = 4;
            bool exceptionThrown = false;
            string errorMessage = String.Empty;

            // Act
            try
            {
                _excelSpreadsheet.GetAmount(sheetName, rowNumber, columnNumber);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.IsTrue(errorMessage.Contains(ReconConsts.MissingCell), $"error message should contain {ReconConsts.MissingCell}");
            Assert.IsTrue(errorMessage.Contains(sheetName), $"error message should contain {sheetName}");
            Assert.IsTrue(errorMessage.Contains($"{rowNumber}"), "error message should contain rowNumber");
            Assert.IsTrue(errorMessage.Contains($"{columnNumber}"), "error message should contain columnNumber");
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadAmountIdentifiedByAmountCode()
        {
            // Arrange
            double expectedAmount = 54.97;
            string sheetName = TestSheetNames.BudgetOut;
            string amountCode = "Code003";

            // Act
            double actualAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode);

            // Assert
            Assert.AreEqual(expectedAmount, actualAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfAmountIdentifiedByAmountCodeIsNotPresent()
        {
            // Arrange
            string sheetName = TestSheetNames.BudgetOut;
            string nonExistentCode = "Does not exist";
            bool exceptionThrown = false;
            string errorMessage = String.Empty;
            string expectedError = string.Format(ReconConsts.MissingCodeInWorksheet, nonExistentCode, sheetName);

            // Act
            try
            {
                _excelSpreadsheet.GetAmount(sheetName, nonExistentCode);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(expectedError, errorMessage);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateDateIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheetName = TestSheetNames.TestRecord;
            int dateRow = 10;
            int dateColumn = 1;
            DateTime previousDate = _excelSpreadsheet.GetDate(sheetName, dateRow, dateColumn);
            DateTime newDate = previousDate.AddDays(1);

            // Act
            _excelSpreadsheet.UpdateDate(sheetName, dateRow, dateColumn, newDate);

            // Assert
            DateTime updatedDate = _excelSpreadsheet.GetDate(sheetName, dateRow, dateColumn);
            Assert.AreEqual(newDate, updatedDate);
            Assert.AreNotEqual(previousDate, updatedDate);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateTextIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheetName = TestSheetNames.TestRecord;
            int textRow = 10;
            int textColumn = 5;
            string previousText = _excelSpreadsheet.GetText(sheetName, textRow, textColumn);
            string newText = previousText + "z";

            // Act
            _excelSpreadsheet.UpdateText(sheetName, textRow, textColumn, newText);

            // Assert
            string updatedText = _excelSpreadsheet.GetText(sheetName, textRow, textColumn);
            Assert.AreEqual(newText, updatedText);
            Assert.AreNotEqual(previousText, updatedText);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountIdentifiedByRowAndColumn()
        {
            // Arrange
            string sheetName = TestSheetNames.TestRecord;
            int amountRow = 10;
            int amountColumn = 2;
            double previousAmount = _excelSpreadsheet.GetAmount(sheetName, amountRow, amountColumn);
            double newAmount = Math.Round(previousAmount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheetName, amountRow, amountColumn, newAmount);

            // Assert
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, amountRow, amountColumn);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreNotEqual(previousAmount, updatedAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountAndTextIdentifiedByAmountCode()
        {
            // Arrange
            string sheetName = TestSheetNames.BudgetOut;
            string amountCode = "Code007";
            int textColumn = 6;
            int amountColumn = 3;
            int row = 9;
            double previousAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, amountColumn);
            string previousText = _excelSpreadsheet.GetText(sheetName, row, textColumn);
            double newAmount = Math.Round(previousAmount + 1.2, 2);
            string newText = previousText + "z";

            // Act
            _excelSpreadsheet.UpdateAmountAndText(sheetName, amountCode, newAmount, newText, amountColumn, textColumn);

            // Assert
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, amountColumn);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreNotEqual(previousAmount, updatedAmount);
            string updatedText = _excelSpreadsheet.GetText(sheetName, row, textColumn);
            Assert.AreEqual(newText, updatedText);
            Assert.AreNotEqual(previousText, updatedText);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanUpdateAmountIdentifiedByAmountCode()
        {
            // Arrange
            string sheetName = TestSheetNames.ExpectedOut;
            string amountCode = "Code005";
            double previousAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, 2);
            double newAmount = Math.Round(previousAmount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheetName, amountCode, newAmount);

            // Assert
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, 2);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreNotEqual(previousAmount, updatedAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void WillThrowSensibleErrorIfTryingToUpdateAmountWithNonExistentAmountCode()
        {
            // Arrange
            string sheetName = TestSheetNames.BudgetOut;
            string nonExistentCode = "Does not exist";
            bool exceptionThrown = false;
            string errorMessage = String.Empty;
            string expectedError = string.Format(ReconConsts.MissingCodeInWorksheet, nonExistentCode, sheetName);

            // Act
            try
            {
                _excelSpreadsheet.UpdateAmount(sheetName, nonExistentCode, 34.56);
            }
            catch (Exception exception)
            {
                exceptionThrown = true;
                errorMessage = exception.Message;
            }

            // Assert
            Assert.IsTrue(exceptionThrown);
            Assert.AreEqual(expectedError, errorMessage);
        }

        //[Ignore("NCrunch has stopped coping with the closing / reopening of a spreadsheet. Runs fine in other test runners.")]
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void UpdatedAmountIsStillPresentAfterSpreadsheetIsClosedAndReopened()
        {
            // Arrange
            const string amountCode = "Code004";
            var sheetName = TestSheetNames.ExpectedOut;
            double previousAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, 2);
            double newAmount = Math.Round(previousAmount + 1.2, 2);

            // Act
            _excelSpreadsheet.UpdateAmount(sheetName, amountCode, newAmount);
            CloseAndReopenSpreadsheet();

            // Assert
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, amountCode, 2);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreNotEqual(previousAmount, updatedAmount);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanGetSubsetOfColumnsAsRowsConvertedToRecords()
        {
            // Arrange
            const int expectedNumItems = 9;
            string sheetName = TestSheetNames.BudgetOut;
            int firstRowNumber = 3;
            int lastRowNumber = 11;
            const int firstColumnNumber = 2;
            const int lastColumnNumber = 6;

            // Act
            List<BankRecord> budgetItems = _excelSpreadsheet.GetRowsAsRecords<BankRecord>(
                sheetName,
                firstRowNumber,
                lastRowNumber,
                firstColumnNumber,
                lastColumnNumber);

            // Assert
            Assert.AreEqual(expectedNumItems, budgetItems.Count);
            Assert.AreEqual("Description001", budgetItems[0].Description);
            Assert.AreEqual("Description009", budgetItems[expectedNumItems - 1].Description);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanInsertNewRow()
        {
            // Arrange
            var sheetName = TestSheetNames.ExpectedOut;
            const int testRowNumber = 12;
            const int amountColumn = 2;
            const int notesColumn = 4;
            int previousNumExpectedOutRows = _excelSpreadsheet.LastRowNumber(sheetName);
            string previousAmountText = _excelSpreadsheet.GetText(sheetName, testRowNumber, amountColumn);
            double newAmount = 79;
            string previousNotesText = _excelSpreadsheet.GetText(sheetName, testRowNumber, notesColumn);
            string newNotes = "Some new notes about some new stuff";
            var cellValues = new Dictionary<int, object>()
            {
                { amountColumn, newAmount },
                { notesColumn, newNotes }
            };

            // Act
            _excelSpreadsheet.InsertNewRow(sheetName, testRowNumber, cellValues);

            // Assert
            int newNumExpectedOutRows = _excelSpreadsheet.LastRowNumber(sheetName);
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, testRowNumber, amountColumn);
            string updatedNotes = _excelSpreadsheet.GetText(sheetName, testRowNumber, notesColumn);
            Assert.AreEqual(previousNumExpectedOutRows + 1, newNumExpectedOutRows);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreNotEqual(previousAmountText, updatedAmount.ToString());
            Assert.AreEqual(newNotes, updatedNotes);
            Assert.AreNotEqual(previousNotesText, updatedNotes);

            // Teardown
            _excelSpreadsheet.DeleteSpecifiedRows(sheetName, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanAppendNewRow()
        {
            // Arrange
            var sheetName = TestSheetNames.CredCard;
            const int testRowNumber = 12;
            const int dateColumn = 1;
            const int amountColumn = 2;
            const int descriptionColumn = 4;
            int previousNumTestRecordRows = _excelSpreadsheet.LastRowNumber(sheetName);
            DateTime newDate = new DateTime(2018, 11, 25);
            double newAmount = 32.45;
            string newDescription = "monthly payment";
            var cellValues = new Dictionary<int, object>()
            {
                { dateColumn, newDate.ToOADate() },
                { amountColumn, newAmount },
                { descriptionColumn, newDescription }
            };

            // Act
            _excelSpreadsheet.AppendNewRow(sheetName, cellValues);

            // Assert
            int newNumTestRecordRows = _excelSpreadsheet.LastRowNumber(sheetName);
            DateTime updatedDate = _excelSpreadsheet.GetDate(sheetName, testRowNumber, dateColumn);
            double updatedAmount = _excelSpreadsheet.GetAmount(sheetName, testRowNumber, amountColumn);
            string updatedDescription = _excelSpreadsheet.GetText(sheetName, testRowNumber, descriptionColumn);
            Assert.AreEqual(previousNumTestRecordRows + 1, newNumTestRecordRows);
            Assert.AreEqual(newDate, updatedDate);
            Assert.AreEqual(newAmount, updatedAmount);
            Assert.AreEqual(newDescription, updatedDescription);

            // Teardown
            _excelSpreadsheet.DeleteSpecifiedRows(sheetName, testRowNumber, testRowNumber);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindLastRowNumberOfSpecifiedSheet()
        {
            // Act
            var result = _excelSpreadsheet.LastRowNumber(TestSheetNames.CredCard);

            // Assert
            Assert.AreEqual(11, result);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadSpecifiedRowGivenRowNumber()
        {
            // Arrange
            var rowNumber = 2;

            // Act
            var result = _excelSpreadsheet.ReadSpecifiedRow(TestSheetNames.ActualBank, rowNumber);

            // Assert
            Assert.AreEqual("Thing", result.ReadCell(1));
            Assert.AreEqual("description", result.ReadCell(2));
            Assert.AreEqual(4567.89, result.ReadCell(3));
            Assert.AreEqual(7898.88, result.ReadCell(4));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanReadSpecifiedRowBasedOnSpecifiedRangeOfColumns()
        {
            // Arrange
            var rowNumber = 2;
            var startColumn = 3;
            var endColumn = 4;

            // Act
            var result = _excelSpreadsheet.ReadSpecifiedRow(TestSheetNames.ActualBank, rowNumber, startColumn, endColumn);

            // Assert
            Assert.AreEqual(2, result.Count, "Number of cells in row");
            Assert.AreEqual("description", result.ReadCell(0));
            Assert.AreEqual(4567.89, result.ReadCell(1));
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void CanFindFirstEmptyRowInColumnForSpecifiedSheet()
        {
            // Arrange
            var columnNumber = 1;

            // Act
            int result = _excelSpreadsheet.FindFirstEmptyRowInColumn(TestSheetNames.TestRecord, columnNumber);

            // Assert
            Assert.AreEqual(13, result);
        }
    }
}