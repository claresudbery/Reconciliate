﻿using ExcelLibrary;
using Interfaces;

namespace ConsoleCatchall.Console.Reconciliation.Spreadsheets
{
    // In the .Net Core version of this file, the factory-factory returns a FakeSpreadsheetFactory.
    internal class SpreadsheetRepoFactoryFactory : ISpreadsheetRepoFactoryFactory
    {
        public ISpreadsheetRepoFactory GetFactory(string spreadsheetFileNameAndPath)
        {
            return new ExcelSpreadsheetRepoFactory(spreadsheetFileNameAndPath);
        }
    }
}