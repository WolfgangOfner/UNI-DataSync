// ----------------------------------------------------------------------- 
// <copyright file="App.xaml.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Method executed on start of the program.
        /// </summary>
        /// <param name="commandLine">Start parameter.</param>
        protected override void OnStartup(StartupEventArgs commandLine)
        {
            if (commandLine.Args.Length != 0)
            {
                if (commandLine.Args.Length == 1)
                {
                    if (commandLine.Args[0].ToLower().Equals("help"))
                    {
                        MessageBox.Show("Following command line arguments are valid: \n\n-) Config file path \n\n-) File size block monitoring, Block size for monitoring \n\n-) Config file path, Log file path \n\n-) Config file path, Log file path, Maximum log file size \n\n-) Config file path, Log file path, Maximum log file size, File size for block monitoring, Block size for monitoring \n\nFollowing values are valid: \n-) Config file path: string \n-) File size block monitoring: double, in Megabyte, greater than zero \n-) Block size: uint, greater than zero \n-) Log file path: string \n-) Maximum log file size: double, in Megabyte, must be greater than zero");
                        Environment.Exit(0);
                    }
                    else
                    {
                        string configFilePath = commandLine.Args[0];

                        if (File.Exists(configFilePath))
                        {
                            MainWindow mainWindow = new MainWindow(configFilePath);
                        }
                        else
                        {
                            MessageBox.Show("Error: Config file not found. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }
                    }                   
                }
                else if (commandLine.Args.Length == 2)
                {
                    double fileSize = 0;
                    int blockSize = 0;

                    try
                    {
                        fileSize = Convert.ToDouble(commandLine.Args[0]);
                        blockSize = Convert.ToInt32(commandLine.Args[1]);
                    }
                    catch (Exception)
                    {
                        // Indicates that config and log file were entered.                    
                    }

                    if (fileSize > 0 && blockSize > 0)
                    {
                        MainWindow mainWindow = new MainWindow(fileSize, blockSize);
                    }
                    else
                    {
                        string configFilePath = commandLine.Args[0];
                        string logFilePath = commandLine.Args[1];

                        if (File.Exists(configFilePath))
                        {
                            MainWindow mainWindow = new MainWindow(configFilePath, logFilePath);
                        }
                        else
                        {
                            MessageBox.Show("Error: Config file not found or block size not greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }
                    }                    
                }
                else if (commandLine.Args.Length == 3)
                {
                    string configFilePath = commandLine.Args[0];

                    double fileSize = 0;
                    int blockSize = 0;

                    try
                    {
                        fileSize = Convert.ToDouble(commandLine.Args[1]);
                        blockSize = Convert.ToInt32(commandLine.Args[2]);
                    }
                    catch (Exception)
                    {
                        // do nothing                        
                    }

                    if (fileSize > 0 && blockSize > 0)
                    {
                        MainWindow mainWindow = new MainWindow(fileSize, blockSize);
                    }
                    else
                    {
                        string logFilePath = commandLine.Args[1];
                        double maxSize = 0;

                        try
                        {
                            maxSize = Convert.ToDouble(commandLine.Args[2]);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error: A porblem with the command line arguments occured. The maximum size of the log file must be greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }

                        if (maxSize <= 0)
                        {
                            MessageBox.Show("Error: Max file size must be greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }

                        if (File.Exists(configFilePath))
                        {
                            MainWindow mainWindow = new MainWindow(configFilePath, logFilePath, maxSize);
                        }
                        else
                        {
                            MessageBox.Show("Error: Config file not found. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }
                    }                    
                }
                else if (commandLine.Args.Length == 5)
                {
                    string configFilePath = commandLine.Args[0];
                    string logFilePath = commandLine.Args[1];
                    double maxSize = 0;
                    double fileSize = 0;
                    int blockSize = 0;

                    try
                    {
                        maxSize = Convert.ToDouble(commandLine.Args[2]);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error: A porblem with the command line arguments occured. The maximum size of the log file must be greater than 0. For help typ in help as command line argument.  The program closes now.", "Error");
                        Environment.Exit(1);
                    }

                    try
                    {
                        fileSize = Convert.ToDouble(commandLine.Args[3]);                        
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error: A porblem with the command line arguments occured. The file size for the block monitoring must be greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                        Environment.Exit(1);
                    }

                    try
                    {
                        blockSize = Convert.ToInt32(commandLine.Args[4]);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error: A porblem with the command line arguments occured. The block size must be greater than 0. For help typ in help as command line argument.", "Error");
                        Environment.Exit(1);
                    }
                        
                        if (maxSize <= 0)
                        {
                            MessageBox.Show("Error: Max file size must be greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }

                    if (fileSize <= 0)
                    {
                        MessageBox.Show("Error: Block file size must be greater than 0. For help typ in help as command line argument.The program closes now.", "Error");
                        Environment.Exit(1);
                    }

                    if (blockSize <= 0)
                    {
                        MessageBox.Show("Error: Block size must be greater than 0. For help typ in help as command line argument. The program closes now.", "Error");
                        Environment.Exit(1);
                    }

                    if (File.Exists(configFilePath))
                        {
                            MainWindow mainWindow = new MainWindow(configFilePath, logFilePath, maxSize);
                        }
                        else
                        {
                            MessageBox.Show("Error: Config file not found. For help typ in help as command line argument. The program closes now.", "Error");
                            Environment.Exit(1);
                        }
                }
                else
                {
                    MessageBox.Show("Error: Wrong amount of comand line arguments inserted. The program closes now. For help typ in help as command line argument.", "Error");
                    Environment.Exit(1);
                }
            }
        }
    }
}
