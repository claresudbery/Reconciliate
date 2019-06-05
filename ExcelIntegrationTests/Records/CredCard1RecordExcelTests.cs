using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Moq;
using NUnit.Framework;

namespace ExcelIntegrationTests.Records
{
    // These are the tests for CredCard1Record
    [TestFixture]
    public partial class ExcelRecordTests //(CredCard1Record)
    {
        // Left this here for research purposes: 
        // This was my attempt DateTimeOffset speed CallConvThiscall test up by using mocking instead of direct file access.
        // But it made it significantly slower!
        //[Test]
        //public void WillPopulateCredCard1RecordCells()
        //{
        //    // Arrange
        //    var credCard1Record = new CredCard1Record();
        //    credCard1Record.Date = new DateTime(year: 2017, month: 4, day: 19);
        //    credCard1Record.Reference = 123456;
        //    credCard1Record.Description = "Acme: Esmerelda's birthday";
        //    credCard1Record.Amount = 1234.56;
        //    var mockCellSet = new Mock<ICellSet>();
        //    var expectedRowNumber = 10;

        //    // Act 
        //    credCard1Record.PopulateSpreadsheetRow(mockCellSet.Object, expectedRowNumber);

        //    // Assert
        //    mockCellSet.Verify(x => x.PopulateCell(expectedRowNumber, CredCard1Record.DateIndex + 1, credCard1Record.Date));
        //    mockCellSet.Verify(x => x.PopulateCell(expectedRowNumber, CredCard1Record.ReferenceIndex + 1, credCard1Record.VendorId));
        //    mockCellSet.Verify(x => x.PopulateCell(expectedRowNumber, CredCard1Record.DescriptionIndex + 1, credCard1Record.Description));
        //    mockCellSet.Verify(x => x.PopulateCell(expectedRowNumber, CredCard1Record.AmountIndex + 1, credCard1Record.Amount));
        //}

        [Ignore("Want to see if any more tests will run slow after all tests using Mock<ICellSet> are ignored.")]
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillPopulateCredCard1RecordCells()
        {
            // Arrange
            var credCard1Record = new CredCard1Record
            {
                Date = new DateTime(year: 2017, month: 4, day: 19),
                Amount = 1234.56,
                Description = "Acme: Esmerelda's birthday"
            };
            var row = 10;
            var mockCells = new Mock<ICellSet>();

            // Act 
            credCard1Record.PopulateSpreadsheetRow(mockCells.Object, row);

            // Assert
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DateSpreadsheetIndex + 1, credCard1Record.Date), "Date");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.AmountSpreadsheetIndex + 1, credCard1Record.MainAmount()), "Amount");
            mockCells.Verify(x => x.PopulateCell(row, CredCard1Record.DescriptionSpreadsheetIndex + 1, credCard1Record.Description), "Desc");
        }

        [Ignore("Trying to find out whether any more tests will run slow when I ignore the mocking ones.")]
        [Test]
        [Parallelizable(ParallelScope.None)]
        public void M_WillReadFromCredCard1RecordCells()
        {
            // Arrange
            var sheetName = "MockSheet";
            DateTime expectedDate = new DateTime(year: 2018, month: 6, day: 1);
            string expectedReference = "55556666";
            String expectedDescription = "description3";
            Double expectedAmount = 37958.90;
            var excelDate = expectedDate.ToOADate();
            var excelDate2 = expectedDate.AddDays(1).ToOADate();
            var fakeCellRow = new FakeCellRow().WithFakeData(new List<object>
            {
                excelDate,
                excelDate2,
                expectedReference,
                expectedDescription,
                expectedAmount
            });
            var mockSpreadsheetRepo = new Mock<ISpreadsheetRepo>();
            mockSpreadsheetRepo.Setup(x => x.ReadLastRow(sheetName)).Returns(fakeCellRow);
            var spreadsheet = new Spreadsheet(mockSpreadsheetRepo.Object);
            var credCard1Record = new CredCard1Record();
            var cells = spreadsheet.ReadLastRow(sheetName);

            // Act 
            credCard1Record.ReadFromSpreadsheetRow(cells);

            // Assert
            Assert.AreEqual(expectedDate, credCard1Record.Date);
            Assert.AreEqual(expectedReference, credCard1Record.Reference);
            Assert.AreEqual(expectedDescription, credCard1Record.Description);
            Assert.AreEqual(expectedAmount, credCard1Record.Amount);
        }
    }
}
