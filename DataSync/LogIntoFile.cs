// ----------------------------------------------------------------------- 
// <copyright file="LogIntoFile.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner..</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Class for logging into a file.
    /// </summary>
   public class LogIntoFile
    {
       /// <summary>
       /// Locker object.
       /// </summary>
       private static object locker = new object();

       /// <summary>
       /// Initializes a new instance of the <see cref="LogIntoFile"/> class.
       /// </summary>
       /// <param name="path">Path to the file.</param>
       /// <param name="maxSize">Maximum size of the file.</param>
        public LogIntoFile(string path, double maxSize)
        {
            FilePath = path;

            // Converts megabytes into bytes.
            MaxSize = maxSize * 1024 * 1024;
            this.CreateFile();
        }

       /// <summary>
       /// Gets or sets the value of the file path.
       /// </summary>
        /// <value>The path of the file.</value>
        public static string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the value of the maximum file size.
        /// </summary>
        /// <value>The value of the file size.</value>
        private static double MaxSize { get; set; }

        /// <summary>
        /// Method to log into the file.
        /// </summary>
        /// <param name="message">Contains logging message.</param>
        public static void Log(string message)
        {
            lock (locker)
            {
                if (!File.Exists(FilePath))
                {
                    MessageBox.Show("Logging file does not exist anymore", "Error");

                    LogIntoTextBox("Logging file does not exist anymore");
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(FilePath);

                    if (fileInfo.Length > MaxSize)
                    {
                        LogIntoTextBox("Max size for log file reached");

                        string pathBakLogFile = Path.GetDirectoryName(FilePath) + "/" + Path.GetFileNameWithoutExtension(FilePath) + ".bak";

                        if (File.Exists(pathBakLogFile))
                        {
                            try
                            {
                                File.Delete(pathBakLogFile);

                                LogIntoTextBox("Old logging .bak file deleted");
                            }
                            catch (Exception ex)
                            {
                                LogIntoTextBox("Error: Can't delete old logging .bak file");
                                LogIntoTextBox(ex.Message);
                            }
                        }

                        try
                        {
                            File.Move(FilePath, pathBakLogFile);

                            LogIntoTextBox("Log File moved to .bak");
                        }
                        catch (Exception ex)
                        {
                            LogIntoTextBox("Error: Can't move log file into .bak");
                            LogIntoTextBox(ex.Message);
                        }

                        try
                        {
                            File.Create(FilePath);

                            LogIntoTextBox("Log file created");
                        }
                        catch (Exception ex)
                        {
                            LogIntoTextBox("Error: Can't create log file");
                            LogIntoTextBox(ex.Message);
                        }
                    }

                    try
                    {
                        using (StreamWriter streamWriter = File.AppendText(FilePath))
                        {
                            streamWriter.WriteLine(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogIntoTextBox("Error: " + ex.Message);
                    }                   
                }
            }
        }
      
       /// <summary>
       /// Method to log into the textbox.
       /// </summary>
       /// <param name="message">Contains logging message.</param>
        private static void LogIntoTextBox(string message)
        {
            LoggingWindow.LogintoTextBox(DateTime.Now + " " + message);
        }

        /// <summary>
        /// Method to create the log file.
        /// </summary>
        private void CreateFile()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath);

                LogIntoTextBox("Log file created");
            }
        }
   }
}