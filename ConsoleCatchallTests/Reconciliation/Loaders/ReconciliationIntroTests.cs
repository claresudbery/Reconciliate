using System;
using System.Collections.Generic;
using ConsoleCatchall.Console.Reconciliation.Records;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using ConsoleCatchallTests.Reconciliation.TestUtils;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ConsoleCatchallTests.Reconciliation.Loaders
{
    [TestFixture]
    public partial class ReconciliationIntroTests : IInputOutput
    {
        private Mock<IInputOutput> _mock_input_output;

        private void Set_up_for_loader_bespoke_stuff(Mock<IInputOutput> mock_input_output, Mock<ISpreadsheet> mock_spreadsheet)
        {
            DateTime last_direct_debit_date = new DateTime(2018, 12, 17);
            var next_direct_debit_date01 = last_direct_debit_date.AddMonths(1);
            var bank_record = new BankRecord { Date = last_direct_debit_date };

            mock_input_output.Setup(x => x.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit,
                ReconConsts.Cred_card2_name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card2_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);

            mock_input_output.Setup(x => x.Get_input(string.Format(
                ReconConsts.AskForCredCardDirectDebit, 
                ReconConsts.Cred_card1_name,
                next_direct_debit_date01.ToShortDateString()), "")).Returns("0");
            mock_spreadsheet.Setup(x => x.Get_most_recent_row_containing_text<BankRecord>(
                    MainSheetNames.Bank_out, ReconConsts.Cred_card1_dd_description, new List<int> { ReconConsts.DescriptionColumn, ReconConsts.DdDescriptionColumn }))
                .Returns(bank_record);
        }

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            TestHelper.Set_correct_date_formatting();
        }

        [SetUp]
        public void Set_up()
        {
            _mock_input_output = new Mock<IInputOutput>();
        }
    }
}
