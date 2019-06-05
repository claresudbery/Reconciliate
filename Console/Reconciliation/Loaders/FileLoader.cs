using System;
using System.Globalization;
using ConsoleCatchall.Console.Reconciliation.Files;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;
using Interfaces.DTOs;

namespace ConsoleCatchall.Console.Reconciliation.Loaders
{
    internal class FileLoader
    {
        public readonly IInputOutput _inputOutput;

        public FileLoader(IInputOutput inputOutput)
        {
            _inputOutput = inputOutput;
        }
    }
}