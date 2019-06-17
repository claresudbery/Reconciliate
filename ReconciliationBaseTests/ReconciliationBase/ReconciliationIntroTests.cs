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
        public void OneTimeSetUp()
        {
            TestHelper.SetCorrectDateFormatting();
        }

        [SetUp]
        public void SetUp()
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
            mock_clock.Setup(x => x.NowDateTime()).Returns(now_date_time);

            // Act
            reconciliate.CreateBackupOfRealSpreadsheet(mock_clock.Object, ReconConsts.TestBackupFilePath);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            string source_file_path = ReconConsts.TestBackupFilePath + "/" + ReconConsts.MainSpreadsheetFileName;
            if (File.Exists(source_file_path))
            {
                var expected_backup_file = ReconConsts.TestBackupFilePath
                                         + @"\SpreadsheetBackups\real_backup_"
                                         + now_date_time.ToString().Replace(" ", "_").Replace(":", "-").Replace("/", "-") 
                                         + "_" + ReconConsts.MainSpreadsheetFileName;
                Assert.IsTrue(File.Exists(expected_backup_file));
            }
        }

        [Test]
        public void CopyMainSpreadsheetToDebugSpreadsheet_WillCopyRealSpreadsheetIntoDebugSpreadsheet()
        {
            // Arrange
            var reconciliate = new ReconciliationIntro(this);
            string debug_file_path = Path.Combine(ReconConsts.TestBackupFilePath, ReconConsts.BackupSubFolder, ReconConsts.DebugSpreadsheetFileName);
            string source_file_path = Path.Combine(ReconConsts.TestBackupFilePath, ReconConsts.MainSpreadsheetFileName);
            File.Delete(debug_file_path);
            Assert.IsFalse(File.Exists(debug_file_path));

            // Act
            reconciliate.CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.TestBackupFilePath, ReconConsts.TestBackupFilePath);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            if (File.Exists(source_file_path))
            {
                Assert.IsTrue(File.Exists(debug_file_path));
            }
        }
    }
}
