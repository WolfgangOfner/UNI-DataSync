// ----------------------------------------------------------------------- 
// <copyright file="LogHandler.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;

    /// <summary>
    /// Class for managing logging.
    /// </summary>
    public static class LogHandler
    {
        /// <summary>
        /// Method for logging.
        /// </summary>
        /// <param name="message">Message for logging.</param>
        public static void Log(string message)
        {
            if (LogIntoFile.FilePath != null)
            {
                LogIntoFile.Log(DateTime.Now + ": " + message);
            }

            LoggingWindow.LogintoTextBox(DateTime.Now + ": " + message);
        }
    }
}