using System;
using System.IO;
using ConsoleCatchall.Console;
using ConsoleCatchall.Console.Reconciliation.Utils;
using Interfaces;
using Interfaces.Constants;
using Moq;
using NUnit.Framework;

namespace ReconciliationBaseTests.ReconciliationBase
{
    [TestFixture]
    public partial class DebugModeSwitcherTests
    {
        [OneTimeSetUp]
        public void One_time_set_up()
        {
            TestHelper.Set_correct_date_formatting();
        }

        private string Mix_slashes(string path)
        {
            var halfway_point = path.IndexOf('\\') != -1 ? path.IndexOf('\\') : path.IndexOf('/');
            halfway_point++;
            var first_half = path.Substring(0, halfway_point);
            var second_half = path.Substring(halfway_point, (path.Length - halfway_point));
            return first_half.Replace('/', '\\') + second_half.Replace('\\', '/');
        }

        [Test]
        public void Create_backup_of_real_spreadsheet__Will_create_backup_in_correct_location_with_date_time_appended()
        {
            // Arrange
            var debugModeSwitcher = new DebugModeSwitcher(new InputOutput());
            var mock_clock = new Mock<IClock>();
            var now_date_time = DateTime.Now;
            mock_clock.Setup(x => x.Now_date_time()).Returns(now_date_time);

            // Act
            debugModeSwitcher.Create_backup_of_real_spreadsheet(mock_clock.Object, ReconConsts.Test_backup_file_path);

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
        public void Create_backup_of_real_spreadsheet__Will_create_backup_even_when_paths_contain_a_mix_of_slashes()
        {
            // Arrange
            var debugModeSwitcher = new DebugModeSwitcher(new InputOutput());
            var mock_clock = new Mock<IClock>();
            var now_date_time = DateTime.Now;
            mock_clock.Setup(x => x.Now_date_time()).Returns(now_date_time);
            var path_with_mixed_slashes = Mix_slashes(ReconConsts.Test_backup_file_path);

            // Act
            debugModeSwitcher.Create_backup_of_real_spreadsheet(mock_clock.Object, path_with_mixed_slashes);

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
        public void Copy_source_spreadsheet_to_debug_spreadsheet__Will_copy_real_spreadsheet_into_debug_spreadsheet()
        {
            // Arrange
            var debugModeSwitcher = new DebugModeSwitcher(new InputOutput());
            string debug_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Backup_sub_folder, ReconConsts.Debug_spreadsheet_file_name);
            string source_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Main_spreadsheet_file_name);
            File.Delete(debug_file_path);
            Assert.IsFalse(File.Exists(debug_file_path));

            // Act
            debugModeSwitcher.Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Test_backup_file_path, ReconConsts.Test_backup_file_path);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            if (File.Exists(source_file_path))
            {
                Assert.IsTrue(File.Exists(debug_file_path));
            }
        }

        [Test]
        public void Copy_source_spreadsheet_to_debug_spreadsheet__Will_copy_spreadsheet_even_when_paths_contain_a_mix_of_slashes()
        {
            // Arrange
            var debugModeSwitcher = new DebugModeSwitcher(new InputOutput());
            string debug_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Backup_sub_folder, ReconConsts.Debug_spreadsheet_file_name);
            string source_file_path = Path.Combine(ReconConsts.Test_backup_file_path, ReconConsts.Main_spreadsheet_file_name);
            File.Delete(debug_file_path);
            Assert.IsFalse(File.Exists(debug_file_path));
            var path_with_mixed_slashes = Mix_slashes(ReconConsts.Test_backup_file_path);

            // Act
            debugModeSwitcher.Copy_source_spreadsheet_to_debug_spreadsheet(path_with_mixed_slashes, path_with_mixed_slashes);

            // Assert
            // This test will fail on the Mac, so don't even bother.
            if (File.Exists(source_file_path))
            {
                Assert.IsTrue(File.Exists(debug_file_path));
            }
        }
    }
}
