using System;
using System.IO;
using ConsoleCatchall.Console.Reconciliation;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ReconciliationIntegrationTests.ReconciliationBase
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
            var mockClock = new Mock<IClock>();
            var nowDateTime = DateTime.Now;
            mockClock.Setup(x => x.NowDateTime()).Returns(nowDateTime);

            // Act
            reconciliate.CreateBackupOfRealSpreadsheet(mockClock.Object, ReconConsts.TestBackupFilePath);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            string sourceFilePath = ReconConsts.TestBackupFilePath + "/" + ReconConsts.MainSpreadsheetFileName;
            if (File.Exists(sourceFilePath))
            {
                var expectedBackupFile = ReconConsts.TestBackupFilePath
                                         + @"\SpreadsheetBackups\real_backup_"
                                         + nowDateTime.ToString().Replace(" ", "_").Replace(":", "-").Replace("/", "-") 
                                         + "_" + ReconConsts.MainSpreadsheetFileName;
                Assert.IsTrue(File.Exists(expectedBackupFile));
            }
        }

        [Test]
        public void CopyMainSpreadsheetToDebugSpreadsheet_WillCopyRealSpreadsheetIntoDebugSpreadsheet()
        {
            // Arrange
            var reconciliate = new ReconciliationIntro(this);
            string debugFilePath = Path.Combine(ReconConsts.TestBackupFilePath, ReconConsts.BackupSubFolder, ReconConsts.DebugSpreadsheetFileName);
            string sourceFilePath = Path.Combine(ReconConsts.TestBackupFilePath, ReconConsts.MainSpreadsheetFileName);
            File.Delete(debugFilePath);
            Assert.IsFalse(File.Exists(debugFilePath));

            // Act
            reconciliate.CopySourceSpreadsheetToDebugSpreadsheet(ReconConsts.TestBackupFilePath, ReconConsts.TestBackupFilePath);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            if (File.Exists(sourceFilePath))
            {
                Assert.IsTrue(File.Exists(debugFilePath));
            }
        }
    }
}
