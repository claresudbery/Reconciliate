using System;
using System.Collections.Generic;
using System.IO;
using ConsoleCatchall.Console.Reconciliation.Spreadsheets;
using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class DebugModeSwitcher
    {
        private readonly IInputOutput _input_output;
        private ISpreadsheetRepoFactory _spreadsheet_factory;

        public DebugModeSwitcher(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public ISpreadsheetRepoFactory Decide_on_debug()
        {
            _input_output.Output_line("");
            _input_output.Output_options(new List<string>
            {
                $"1. Debug Mode A: Copy live sheet to debug version in [live location]/{ReconConsts.Backup_sub_folder}, and work on it from there.",
                $"2. Debug Mode B: Copy sheet from {ReconConsts.Source_debug_spreadsheet_path} to [live location]/{ReconConsts.Backup_sub_folder}, and work on it from there.",
                "3. Debug Mode C: Use fake spreadsheet repo (like you would get in .Net Core).",
                "4. Work in REAL mode"
            });

            string input = _input_output.Get_generic_input(ReconConsts.DebugOrReal);

            WorkingMode working_mode = WorkingMode.DebugA;
            switch (input)
            {
                case "1": { working_mode = WorkingMode.DebugA; Debug_mode_a(); } break;
                case "2": { working_mode = WorkingMode.DebugB; Debug_mode_b(); } break;
                case "3": { working_mode = WorkingMode.DebugC; Debug_mode_c(); } break;
                case "4": { working_mode = WorkingMode.Real; Real_mode(); } break;
            }

            new Communicator(_input_output).Show_instructions(working_mode);

            return _spreadsheet_factory;
        }

        public void Debug_mode_a()
        {
            Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Main_spreadsheet_path, ReconConsts.Main_spreadsheet_path);
            string debug_file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path,
                ReconConsts.Backup_sub_folder,
                ReconConsts.Debug_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(debug_file_path);
        }

        public void Debug_mode_b()
        {
            Copy_source_spreadsheet_to_debug_spreadsheet(ReconConsts.Source_debug_spreadsheet_path, ReconConsts.Main_spreadsheet_path);
            string debug_file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path,
                ReconConsts.Backup_sub_folder,
                ReconConsts.Debug_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(debug_file_path);
        }

        public void Debug_mode_c()
        {
            _spreadsheet_factory = new FakeSpreadsheetRepoFactory();
        }

        private void Real_mode()
        {
            Create_backup_of_real_spreadsheet(new Clock(), ReconConsts.Main_spreadsheet_path);
            string file_path = Path.Combine(
                ReconConsts.Main_spreadsheet_path,
                ReconConsts.Main_spreadsheet_file_name);
            _spreadsheet_factory = new SpreadsheetRepoFactoryFactory().Get_factory(file_path);
        }

        public void Copy_source_spreadsheet_to_debug_spreadsheet(string source_spreadsheet_path, string main_spreadsheet_path)
        {
            string source_file_path = Path.Combine(source_spreadsheet_path, ReconConsts.Main_spreadsheet_file_name);
            if (File.Exists(source_file_path))
            {
                string debug_file_path = Path.Combine(
                    main_spreadsheet_path,
                    ReconConsts.Backup_sub_folder,
                    ReconConsts.Debug_spreadsheet_file_name);
                Copy_file(source_file_path, debug_file_path);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        public void Create_backup_of_real_spreadsheet(IClock clock, string spreadsheet_path)
        {
            string source_file_path = Path.Combine(spreadsheet_path, ReconConsts.Main_spreadsheet_file_name);
            if (File.Exists(source_file_path))
            {
                string file_name_prefix = $"{ReconConsts.Backup_sub_folder}\\real_backup_";
                file_name_prefix = file_name_prefix + clock.Now_date_time();
                file_name_prefix = file_name_prefix.Replace(" ", "_").Replace(":", "-").Replace("/", "-");
                string backup_file_name = file_name_prefix + "_" + ReconConsts.Main_spreadsheet_file_name;
                string backup_file_path = spreadsheet_path + "\\" + backup_file_name;

                Copy_file(source_file_path, backup_file_path);
            }
            else
            {
                throw new Exception($"Can't find file: {source_file_path}");
            }
        }

        private void Copy_file(string source_file_path, string dest_file_path)
        {
            try
            {
                File.Copy(source_file_path, dest_file_path, true);
            }
            catch (System.IO.FileNotFoundException e)
            {
                // There seems to be an intermittent fault in Windows - sometimes it copes with mixed slashes and sometimes it doesn't!
                source_file_path = source_file_path.Replace("/", "\\");
                dest_file_path = dest_file_path.Replace("/", "\\");

                File.Copy(source_file_path, dest_file_path, true);
            }
        }
    }
}