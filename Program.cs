﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace QuestStudio
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
      {
     /* AppDomain.CurrentDomain.UnhandledException +=
        new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      Application.ThreadException +=
        new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);*/
      Application.EnableVisualStyles( );
      Application.SetCompatibleTextRenderingDefault(false);

      if (args != null && args.Length > 0)
      {
          Application.Run(new TreeForm(args[0]));
      }
      else
      {
          Application.Run(new TreeForm());
      }
    }


    static void CurrentDomain_UnhandledException
        (object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Exception ex = (Exception)e.ExceptionObject;

            MessageBox.Show("Whoops! Please contact the developers with "
               + "the following information:\n\n" + ex.Message + ex.StackTrace,
               "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
        finally
        {
            Application.Exit();
        }
    }

    public static void Application_ThreadException
      (object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        DialogResult result = DialogResult.Abort;
        try
        {
            result = MessageBox.Show("Whoops! Please contact the developers "
              + "with the following information:\n\n" + e.Exception.Message
              + e.Exception.StackTrace, "Application Error",
              MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }
        finally
        {
            if (result == DialogResult.Abort)
            {
                Application.Exit();
            }
        }
    }
  }

    public class FooException: Exception
    {
        public FooException()
        {
        }

        public FooException(string message)
            : base(message)
        {
        }

        public FooException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
