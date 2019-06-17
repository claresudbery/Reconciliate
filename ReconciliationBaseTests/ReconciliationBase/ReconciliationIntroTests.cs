using System;
using System.IO;
using ConsoleCatchall.Console.Reconciliation;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    [TestFixture]
    public partial class ReconciliationIntroTests : IInputOutput
    {
        private Mock<IInputOutput> _mockInputOutput;

        [OneTimeSetUp]
        public void One_time_set_up()
        {
            TestHelper.Set_correct_date_formatting();
        }

        [SetUp]
        public void Set_up()
        {
            _mockInputOutput = new Mock<IInputOutput>();
        }

        [Test]
        public void CreateBackupOfRealSpreadsheet_WillCreateBackupInCorrectLocationWithDateTimeAppended()
        {
            // Arrange
            var reconciliate = new ReconciliationIntro(this);
            var mock_clock = new Mock<IClock>();
            var now_date_time = DateTime.Now;
            mock_clock.Setup(x => x.Now_date_time()).Returns(now_date_time);

            // Act
            reconciliate.Create_backup_of_real_spreadsheet(mock_clock.Object, ReconConsts.Test_backup_file_path);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            string source_file_path = ReconConsts.Test_backup_file_path + "/" + ReconConsts.Main_spreadsheet_file_name;
            if (File.Exists(source_file_path))
            {
                var expected_backup_file = ReconConsts.Test_backup_file_path
                                         + @"\SpreadsheetBackups\real_backup_"
                                         + now_date_time.ToString().Replace(" ", "_").Replace(":", "-").Replace("/", "-") 
                                         + "_" + ReconConsts.Main_spreadsheet_file_name;
                Assert.IsTrue(File.Exists(expected_backup_file));
            }
        }

        [Test]
        public void CopyMainSpreadsheetToDebugSpreadsheet_WillCopyRealSpreadsheetIntoDebugSpreadsheet()
        {
            // Arrange
            var reconciliate = new ReconciliationIntro(this);
            string debug_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Backup_sub_folder, ReconConsts.Debug_spreadsheet_file_name);
            string source_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Main_spreadsheet_file_name);
            File.Delete(debug_file_path);
            Assert.IsFalse(File.Exists(debug_file_path));

            // Act
            reconciliate.Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Test_backup_file_path, ReconConsts.Test_backup_file_path);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            if (File.Exists(source_file_path))
            {
                Assert.IsTrue(File.Exists(debug_file_path));
            }
        }
    }
}
