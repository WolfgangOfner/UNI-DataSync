// ----------------------------------------------------------------------- 
// <copyright file="ManageLogFile.xaml.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{   
    using System;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for ManageLogFile.
    /// </summary>
    public partial class ManageLogFile : Window
    {
        /// <summary>
        /// Path of the log file.
        /// </summary>
        private string path = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageLogFile"/> class.
        /// </summary>
        public ManageLogFile()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Method to check inputs.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {            
            double maxSize;

            if (this.path == string.Empty)
            {
                MessageBox.Show("Please select a path for the logging File");
            }
            else
            {
                if (tbSize.Text != string.Empty)
                {
                    try
                    {
                        maxSize = Convert.ToDouble(tbSize.Text);

                        if (maxSize <= 0 || maxSize > 50000)
                        {
                            MessageBox.Show("Size musst be a positive number and smaller than 50000");

                            LogHandler.Log("Error: Max size must be a positive number and smaller than 50000");
                        }
                        else
                        {
                            LogIntoFile logIntoFile = new LogIntoFile(this.path, maxSize);

                            LogHandler.Log("Log file defined and config window for log file closed");

                            this.Close();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Size musst be a positive number");

                        LogHandler.Log("Error: Not a positive number entered as file max size");
                    }
                }
            }            
        }

        /// <summary>
        /// Method to cancel input.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Config window for log file closed");

            this.Close();
        }

        /// <summary>
        /// Method to add the path of the log file.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnPath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.OverwritePrompt = false;
            saveFileDialog.ShowDialog();

            this.path = saveFileDialog.FileName;                     
        }

        /// <summary>
        /// Method for logging when config window is loaded.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void Manage_Logging_File_Loaded(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Config window for log file opened");
        }
    }
}