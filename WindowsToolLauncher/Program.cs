
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PInvoke.Controls;

namespace WindowsTool
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm form;

            switch (args.Length)
            {
                case 0: form = new MainForm(); break;

                case 1:
                    {
                        string full_path;
                        try
                        {
                            full_path = System.IO.Path.GetFullPath(args[0]);
                        }
                        catch (Exception)
                        {
                            goto default;
                        }

                        form = new MainForm(full_path);
                        break;
                    }

                default:
                    {
                        MessageBox.Show(
                            "Usage: winsiggen [<path_to_assembly>]",
                            "Unrecognized command line",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
            }

            Application.Run(form);
        }
    }
}
