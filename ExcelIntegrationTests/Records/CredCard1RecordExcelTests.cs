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
        // This was my attempt to speed this test up by using mocking instead of direct file access.
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
    }
}
