using Interfaces;
using Interfaces.Constants;

namespace ConsoleCatchall.Console.Reconciliation.Utils
{
    internal class Communicator
    {
        private readonly IInputOutput _input_output;
        public Communicator(IInputOutput input_output)
        {
            _input_output = input_output;
        }

        public void Show_instructions(WorkingMode working_mode)
        {
            _input_output.Output_line("Here's how it works:");
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_01);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_02);
            _input_output.Output_line(ReconConsts.Instructions_line_03);
            _input_output.Output_line(ReconConsts.Instructions_line_04);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_05);
            _input_output.Output_line(ReconConsts.Instructions_line_06);
            _input_output.Output_line(ReconConsts.Instructions_line_07);
            _input_output.Output_line(ReconConsts.Instructions_line_08);
            _input_output.Output_line(ReconConsts.Instructions_line_09);
            _input_output.Output_line("                                      ****");
            _input_output.Output_line(ReconConsts.Instructions_line_10);
            _input_output.Output_line(ReconConsts.Instructions_line_11);
            _input_output.Output_line("");

            switch (working_mode)
            {
                case WorkingMode.DebugA: Show_debug_a_data_message(); break;
                case WorkingMode.DebugB: Show_debug_b_data_message(); break;
                case WorkingMode.DebugC: Show_debug_c_data_message(); break;
                case WorkingMode.Real: Show_real_data_message(); break;
            }
        }

        private void Show_debug_a_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE A!                     ***");
            _input_output.Output_line("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("*** It's an up to date copy of the main spreadsheet. It lives here: ***");
            _input_output.Output_line($"***  {ReconConsts.Main_spreadsheet_path}/{ReconConsts.Backup_sub_folder}  ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***  You can find debug versions of all csv files and a spreadsheet ***");
            _input_output.Output_line($"***     in [project root]/reconciliation-samples/For debugging     ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_debug_b_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE B!                     ***");
            _input_output.Output_line("***                 DEBUG SPREADSHEET WILL BE USED!                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***      We'll copy the spreadsheet from the following folder:      ***");
            _input_output.Output_line($"***                {ReconConsts.Source_debug_spreadsheet_path}                 ***");
            _input_output.Output_line("***              The working copy will be placed here:              ***");
            _input_output.Output_line($"***  {ReconConsts.Main_spreadsheet_path}/{ReconConsts.Backup_sub_folder}  ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***  You can find debug versions of all csv files and a spreadsheet ***");
            _input_output.Output_line($"***     in [project root]/reconciliation-samples/For debugging     ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_debug_c_data_message()
        {
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                    WORKING IN DEBUG MODE C!                     ***");
            _input_output.Output_line("***                  NO SPREADSHEET WILL BE USED!                   ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***                 Using fake spreadsheet factory.                 ***");
            _input_output.Output_line("***                                                                 ***");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("***********************************************************************");
            _input_output.Output_line("");
        }

        private void Show_real_data_message()
        {
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("***                            WORKING IN REAL MODE!                            ***");
            _input_output.Output_line("***                      REAL SPREADSHEET WILL BE UPDATED!                      ***");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("*** (unless you're in .Net Core, in which case you're in debug mode by default) ***");
            _input_output.Output_line("***                                                                             ***");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("***********************************************************************************");
            _input_output.Output_line("");
        }
    }
}