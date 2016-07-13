// ----------------------------------------------------------------------- 
// <copyright file="LoggingWindow.xaml.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner..</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for LoggingWindow.
    /// </summary>
    public partial class LoggingWindow : Window
    {
        /// <summary>
        /// List with all the logging strings.
        /// </summary>
        private static List<string> loggingFile = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingWindow"/> class.
        /// </summary>
        public LoggingWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Method to save the logging text. 
        /// </summary>
        /// <param name="message">Contains the logging message.</param>
        public static void LogintoTextBox(string message)
        {
            loggingFile.Add(message);            
        }

        /// <summary>
        /// Method to write logging list into the textbox when window opens.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Method event args.</param>
        private void TbLogging_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in loggingFile)
            {
                tbLogging.AppendText(item);
                tbLogging.AppendText(Environment.NewLine);
            }
        }
    }
}