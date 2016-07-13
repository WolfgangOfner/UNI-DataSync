// ----------------------------------------------------------------------- 
// <copyright file="MainWindow.xaml.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Xml;

    /// <summary>
    /// Interaction logic for MainWindow.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// List with all the jobs.
        /// </summary>
        private static List<LoggingJob> loggingJobList = new List<LoggingJob>();

        /// <summary>
        /// List with the threads.
        /// </summary>
        private static List<Thread> workingThreads = new List<Thread>();

        /// <summary>
        /// List with the file watcher.
        /// </summary>
        private static List<FileWatcher> fileWatcherList = new List<FileWatcher>();

        /// <summary>
        /// Minimum file size to activate block monitoring.
        /// </summary>
        private static double blockFileSize;

        /// <summary>
        /// Size of the monitoring block.
        /// </summary>
        private static int blockSize;

        /// <summary>
        /// List with the exception paths.
        /// </summary>
        private List<FilePath> exceptionPathList = new List<FilePath>();

        /// <summary>
        /// List with the destination paths.
        /// </summary>
        private List<FilePath> destinationPathList = new List<FilePath>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            LogHandler.Log("Program started");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="configFilePath">Path to config file.</param>
        public MainWindow(string configFilePath)
        {
            this.InitializeComponent();
            LogHandler.Log("Program started");
            this.ReadXML(configFilePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="fileSizeCommandLine">Block file size when monitoring starts.</param>
        /// <param name="blockSizeCommandLine">Block size.</param>
        public MainWindow(double fileSizeCommandLine, int blockSizeCommandLine)
        {
            this.InitializeComponent();
            LogHandler.Log("Program started");
            blockFileSize = fileSizeCommandLine;
            blockSize = blockSizeCommandLine;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="configFilePath">Path to config file.</param>
        /// <param name="logFilePath">Path to log file.</param>
        /// <param name="maxSize">Maximum size of log file (maxValue / 1024 / 1024 because later * 1024 *1024).</param>
        public MainWindow(string configFilePath, string logFilePath, double maxSize = double.MaxValue / 1024 / 1024)
        {
            this.InitializeComponent();
            LogIntoFile logIntoFile = new LogIntoFile(logFilePath, maxSize);
            LogHandler.Log("Program started");
            this.ReadXML(configFilePath);
        }

        /// <summary>
        /// Method to open the log file managing window.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void ManageLoggingFile_Click(object sender, RoutedEventArgs e)
        {
            ManageLogFile manageLogFile = new ManageLogFile();
            manageLogFile.ShowDialog();
        }

        /// <summary>
        /// Method to set the source.
        /// </summary>     
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnSource_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = "C:\\";

            DialogResult result = folderDialog.ShowDialog();

            // if user selects a folder
            if (result.ToString() == "OK")
            {
                tbSource.Text = folderDialog.SelectedPath;
                btnException.IsEnabled = true;
                btnDestination.IsEnabled = true;
                btnClearEntry.IsEnabled = true;

                LogHandler.Log(folderDialog.SelectedPath + " as source selected");
            }
        }

        /// <summary>
        /// Method to set exceptions.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnException_Click(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            string source = tbSource.Text;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = source;
            folderDialog.Description = "Select exception directory";

            DialogResult result = folderDialog.ShowDialog();

            // if user selects a folder
            if (result.ToString() == "OK")
            {
                path = folderDialog.SelectedPath;

                if (path.ToLower().Contains(source.ToLower()) && !path.ToLower().Equals(source.ToLower()))
                {
                    if (this.exceptionPathList.Count == 0)
                    {
                        FilePath exception = new FilePath(folderDialog.SelectedPath);
                        this.exceptionPathList.Add(exception);

                        this.UpdateGrid(1);

                        LogHandler.Log(folderDialog.SelectedPath + " as exception added");
                    }
                    else
                    {
                        bool add = true;

                        for (int i = 0; i < this.exceptionPathList.Count; i++)
                        {
                            if (this.exceptionPathList[i].Path.Contains(path + "\r"))
                            {
                                add = false;
                                break;
                            }
                        }

                        if (add)
                        {
                            FilePath exceptionFilePath = new FilePath(folderDialog.SelectedPath);
                            this.exceptionPathList.Add(exceptionFilePath);

                            this.UpdateGrid(1);

                            LogHandler.Log(folderDialog.SelectedPath + " as exception added");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Exception already selected", "Error");

                            LogHandler.Log(folderDialog.SelectedPath + " already selected and not added to the exception list");
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Exception must be a subdirectory of the source", "Error");

                    LogHandler.Log(folderDialog.SelectedPath + " is not a subdirectory of the source and not added to the exception list");
                }
            }
        }

        /// <summary>
        /// Method to set destination.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnDestination_Click(object sender, RoutedEventArgs e)
        {
            string source = tbSource.Text;
            string destination = string.Empty;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = "C:\\";
            bool add = true;

            DialogResult result = folderDialog.ShowDialog();

            // if user selects a folder
            if (result.ToString() == "OK")
            {
                destination = folderDialog.SelectedPath;

                if (!destination.Contains(source + @"\"))
                {
                    if (this.destinationPathList.Count == 0)
                    {
                        FilePath destinationFilePath = new FilePath(folderDialog.SelectedPath);

                        this.destinationPathList.Add(destinationFilePath);

                        this.UpdateGrid(2);

                        btnAddEntry.IsEnabled = true;

                        LogHandler.Log(folderDialog.SelectedPath + " as destination added");
                    }
                    else
                    {
                        for (int i = 0; i < this.destinationPathList.Count; i++)
                        {
                            if (this.destinationPathList[i].Path.Equals(destination))
                            {
                                add = false;
                                break;
                            }
                        }

                        if (add)
                        {
                            FilePath ex = new FilePath(folderDialog.SelectedPath);
                            this.destinationPathList.Add(ex);

                            this.UpdateGrid(2);

                            LogHandler.Log(folderDialog.SelectedPath + " as destination added");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Destination already selected", "Error");

                            LogHandler.Log(folderDialog.SelectedPath + " already selected as destination and not added to the destination list");
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Destination must not be a subdirectory of the source", "Error");

                    LogHandler.Log(folderDialog.SelectedPath + " is a subdirectory of the source and is not added to the destination list");
                }
            }
        }

        /// <summary>
        /// Method to update exception and destination grid.
        /// </summary>
        /// <param name="mode">Indicates which grid should be updated.</param>
        private void UpdateGrid(int mode)
        {
            if (mode == 1)
            {
                dgException.ItemsSource = null;
                dgException.ItemsSource = this.exceptionPathList;
            }
            else if (mode == 2)
            {
                dgDestination.ItemsSource = null;
                dgDestination.ItemsSource = this.destinationPathList;
            }
        }

        /// <summary>
        /// Method to clear UI.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnClearEntry_Click(object sender, RoutedEventArgs e)
        {
            tbSource.Text = string.Empty;
            this.exceptionPathList.Clear();
            this.destinationPathList.Clear();
            dgException.ItemsSource = null;
            dgDestination.ItemsSource = null;
            btnException.IsEnabled = false;
            btnDestination.IsEnabled = false;
            btnAddEntry.IsEnabled = false;

            LogHandler.Log("Entry cleared");
        }

        /// <summary>
        /// Method to import config file.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void MiImportConfig_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = openFileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                LogHandler.Log(openFileDialog.FileName + " as config file selected");

                this.ReadXML(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Method to add entry to the job list.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnAddEntry_Click(object sender, RoutedEventArgs e)
        {
            List<string> exceptionPath = new List<string>();
            List<string> destinationPath = new List<string>();

            // copy all exceptions and destination into string list
            foreach (var item in this.exceptionPathList)
            {
                exceptionPath.Add(item.Path);
            }

            foreach (var item in this.destinationPathList)
            {
                destinationPath.Add(item.Path);
            }

            LoggingJob loggingJob = new LoggingJob(tbSource.Text, cbRootOnly.IsChecked.Value, exceptionPath, destinationPath);
            loggingJobList.Add(loggingJob);

            LogHandler.Log("Entry added to job list");

            miManageLoggingFile.IsEnabled = false;
            btnStartSync.IsEnabled = true;
            miImportConfig.IsEnabled = false;
            this.ClearGui();
        }

        /// <summary>
        /// Method to reset the GUI.
        /// </summary>
        private void ClearGui()
        {
            tbSource.Text = string.Empty;
            this.exceptionPathList.Clear();
            this.destinationPathList.Clear();
            dgException.ItemsSource = null;
            dgDestination.ItemsSource = null;
            btnException.IsEnabled = false;
            btnDestination.IsEnabled = false;
            btnClearEntry.IsEnabled = false;
            btnAddEntry.IsEnabled = false;
            miManageLoggingFile.IsEnabled = false;
            cbRootOnly.IsChecked = false;

            LogHandler.Log("Gui cleared");
        }

        /// <summary>
        /// Method to reset the program.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            loggingJobList.Clear();

            LogHandler.Log("Job list cleared");

            miImportConfig.IsEnabled = true;
            btnStartSync.IsEnabled = false;
            this.ClearGui();
            miManageLoggingFile.IsEnabled = true;
            cbBlock.IsEnabled = false;
            tbFileSize.IsEnabled = false;
            tbBlockSize.IsEnabled = false;
            tbFileSize.Text = "0";
            tbBlockSize.Text = "0";

            LogHandler.Log("Program resetted");
        }

        /// <summary>
        /// Method to start the sync. 
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void BtnStartSync_Click(object sender, RoutedEventArgs e)
        {
            bool error = false;

            if (cbBlock.IsChecked.Value)
            {
                blockSize = 0;
                blockFileSize = 0;

                try
                {
                    blockFileSize = Convert.ToDouble(tbFileSize.Text);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Error: File size must be a positive double value, greater than 0.", "Error");

                    LogHandler.Log("Error: File size must be a positive double value, greater than 0.");
                }

                try
                {
                    blockSize = Convert.ToInt32(tbBlockSize.Text);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Error: Block size must be a positive int value, greater than 0.", "Error");

                    LogHandler.Log("Error: Block size must be a positive int value, greater than 0.");
                }

                if (blockFileSize > 0 && blockSize > 0)
                {
                    blockFileSize *= 1024 * 1024;

                    FileWatcher fileWatcher = new FileWatcher(blockFileSize, blockSize);
                }
                else
                {
                    error = true;

                    System.Windows.MessageBox.Show("Error: File size and block size must be greater than 0.", "Error");

                    LogHandler.Log("Error: File size and block size must be greater than 0.");
                }
            }

            if (!error)
            {
                btnAddEntry.IsEnabled = false;
                btnReset.IsEnabled = false;
                btnClearEntry.IsEnabled = false;
                btnStartSync.IsEnabled = false;
                btnSource.IsEnabled = false;
                cbRootOnly.IsEnabled = false;
                cbBlock.IsEnabled = false;
                tbFileSize.IsEnabled = false;
                tbBlockSize.IsEnabled = false;

                this.StartMonitoring();
            }            
        }

        /// <summary>
        /// Method to start monitoring the sources.
        /// </summary>
        private void StartMonitoring()
        {
            LogHandler.Log("Monitoring started");

            for (int i = 0; i < loggingJobList.Count; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(this.WorkerThread));

                t.IsBackground = true;

                t.Start((object)i);

                workingThreads.Add(t);

                LogHandler.Log("Thread started");
            }
        }

        /// <summary>
        /// Method to create a fileWatcher for monitoring.
        /// </summary>
        /// <param name="obj">Contains index of the loggingJobList.</param>
        private void WorkerThread(object obj)
        {
            FileWatcher fileWatcher = new FileWatcher(loggingJobList[Convert.ToInt32(obj)]);
            fileWatcherList.Add(fileWatcher);
        }

        /// <summary>
        /// Read the xml config file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        private void ReadXML(string path)
        {
            LogHandler.Log("Reading XML file started");

            string source = string.Empty;
            bool rootOnly = false;
            bool blockEnabled = false;
            string logFilePath = string.Empty;
            double maxLogFileSize = 0;
            List<string> exception = new List<string>();
            List<string> destination = new List<string>();

            XmlTextReader reader = new XmlTextReader(path);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    // Opening Tag
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "Source":
                                reader.Read();

                                if (!source.Equals(string.Empty))
                                {
                                    System.Windows.MessageBox.Show("Error: Multiple source paths found. The program closes now", "Error");

                                    LogHandler.Log("Error: Multiple source paths found");

                                    Environment.Exit(1);
                                }

                                source = reader.Value.Trim();

                                if (!Directory.Exists(source))
                                {
                                    System.Windows.MessageBox.Show("Error: Source path not found. The program closes now", "Error");

                                    LogHandler.Log("Error: Source path not found");

                                    Environment.Exit(1);
                                }

                                break;
                            case "RootOnly":
                                reader.Read();

                                try
                                {
                                    rootOnly = Convert.ToBoolean(reader.Value.Trim());
                                }
                                catch (Exception)
                                {
                                    System.Windows.MessageBox.Show("Error: Root only value is not true or false. The program closes now", "Error");

                                    LogHandler.Log("Error: Root only value is not true or false");

                                    Environment.Exit(1);
                                }

                                break;

                            case "Exception":
                                reader.Read();
                                string exceptionPath = reader.Value.Trim();

                                if (Directory.Exists(exceptionPath))
                                {
                                    if (exceptionPath.Contains(source) && !exceptionPath.ToLower().Equals(source.ToLower()))
                                    {
                                        exception.Add(exceptionPath);
                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show("Error: Exception path not a subdirectory of the source. The program closes now", "Error");

                                        LogHandler.Log("Error: Exception path is not a subdirectory of the source");

                                        Environment.Exit(1);
                                    }
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Error: Exception path not found. The program closes now", "Error");

                                    LogHandler.Log("Error: Exception path not found");

                                    Environment.Exit(1);
                                }

                                break;

                            case "Destination":
                                reader.Read();
                                string filePath = reader.Value.Trim();
                                
                                if (Directory.Exists(filePath))
                                {
                                    if (filePath.Contains(source + @"\"))
                                    {
                                        System.Windows.MessageBox.Show("Error: Destination path is a subdirectory of the source. The program closes now", "Error");

                                        LogHandler.Log("Error: Destination path is a subdirectory of the source");

                                        Environment.Exit(1);
                                    }
                                    else
                                    {
                                        bool add = true;

                                        for (int i = 0; i < destination.Count; i++)
                                        {
                                            if (destination[i].Equals(filePath))
                                            {
                                                add = false;
                                                break;
                                            }
                                        }

                                        if (add)
                                        {
                                            destination.Add(filePath);

                                            LogHandler.Log(filePath + " as destination added");
                                        }
                                        else
                                        {
                                            LogHandler.Log(filePath + " already selected as destination and not added to the destination list");
                                        }
                                    }
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Error: Destination path not found. The program closes now", "Error");

                                    LogHandler.Log("Error: Destination path not found");

                                    Environment.Exit(1);
                                }

                                break;

                            case "DataSync":
                                break;

                            case "Block":
                                break;

                            case "BlockEnabled":
                                reader.Read();

                                if (reader.Value.Trim().ToLower().Equals("true"))
                                {
                                    blockEnabled = true;
                                }

                                break;

                            case "BlockFileSize":
                                reader.Read();

                                if (blockEnabled)
                                {
                                    try
                                    {
                                        blockFileSize = Convert.ToDouble(reader.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        System.Windows.MessageBox.Show("Error: Wrong value in BlockFileSize tag. The program closes now", "Error");

                                        LogHandler.Log("Error: Wrong value in BlockFileSize tag.");

                                        Environment.Exit(1);
                                    }

                                    if (blockFileSize <= 0)
                                    {
                                        System.Windows.MessageBox.Show("Error: BlockFileSize must be greater than 0. The program closes now", "Error");

                                        LogHandler.Log("Error: BlockFileSize must be greater than 0.");

                                        Environment.Exit(1);
                                    }
                                }

                                break;

                            case "BlockSize":
                                reader.Read();

                                if (blockEnabled)
                                {
                                    try
                                    {
                                        blockSize = Convert.ToInt32(reader.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        System.Windows.MessageBox.Show("Error: Wrong value in BlockSize tag. The program closes now", "Error");

                                        LogHandler.Log("Error: Wrong value in BlockSize tag.");

                                        Environment.Exit(1);
                                    }

                                    if (blockSize <= 0)
                                    {
                                        System.Windows.MessageBox.Show("Error: BlockSize must be greater than 0. The program closes now", "Error");

                                        LogHandler.Log("Error: BlockSize must be greater than 0.");

                                        Environment.Exit(1);
                                    }
                                }

                                break;

                            case "Log":
                                break;

                            case "LogFile":
                                reader.Read();

                                logFilePath = reader.Value.Trim();

                                if (!File.Exists(logFilePath))
                                {
                                    try
                                    {
                                        File.Create(logFilePath);
                                    }
                                    catch (Exception)
                                    {
                                        System.Windows.MessageBox.Show("Error: Can't create log file. The program closes now", "Error");

                                        LogHandler.Log("Error: Can't create log file.");

                                        Environment.Exit(1);
                                    }
                                }

                                break;

                            case "LogSize":
                                reader.Read();

                                if (!logFilePath.Equals(string.Empty))
                                {
                                    try
                                    {
                                        maxLogFileSize = Convert.ToInt32(reader.Value.Trim());
                                    }
                                    catch (Exception)
                                    {
                                        System.Windows.MessageBox.Show("Error: Wrong value in LogSize tag. The program closes now", "Error");

                                        LogHandler.Log("Error: Wrong value in LogSize tag.");

                                        Environment.Exit(1);
                                    }

                                    if (maxLogFileSize <= 0)
                                    {
                                        System.Windows.MessageBox.Show("Error: LogSize must be greater than 0. The program closes now", "Error");

                                        LogHandler.Log("Error: LogSize must be greater than 0.");

                                        Environment.Exit(1);
                                    }
                                    else
                                    {
                                        LogIntoFile logIntoFile = new LogIntoFile(logFilePath, maxLogFileSize);
                                    }
                                }

                                break;

                            case "SyncJob":
                                break;

                            default:
                                System.Windows.MessageBox.Show("Error: Wrong tag found in xml config file. The program closes now", "Error");

                                LogHandler.Log("Error: Wrong tag found in xml config file");

                                Environment.Exit(1);
                                break;
                        }

                        break;

                    // Closing Tag
                    case XmlNodeType.EndElement:

                        if (reader.Name.Equals("SyncJob"))
                        {
                            if (!source.Equals(string.Empty) && !destination.Equals(string.Empty))
                            {
                                loggingJobList.Add(new LoggingJob(source, rootOnly, exception, destination));

                                LogHandler.Log("Logging job created and added to the job list");

                                source = string.Empty;
                                exception.Clear();
                                destination.Clear();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Error: Source or destination path missing. The program closes now", "Error");

                                LogHandler.Log("Error: Source or destination path not found");

                                Environment.Exit(1);
                            }
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Method to check the amount of entries in the destination grid.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void DgDestination_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // 1 because element isn't deleted yet
                if (this.destinationPathList.Count == 1)
                {
                    btnAddEntry.IsEnabled = false;

                    LogHandler.Log("Last element deleted from the destination list");
                }
                else
                {
                    LogHandler.Log("Element deleted from the destination list");
                }
            }
        }

        /// <summary>
        /// Method to show textbox with the logs.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void MiShowLogging_Click(object sender, RoutedEventArgs e)
        {
            LoggingWindow loggingWindow = new LoggingWindow();

            LogHandler.Log("Logging window opened");

            loggingWindow.Show();
        }

        /// <summary>
        /// Method to check updates in exception gris.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void DgException_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            this.UpdateDatagrid(e, 1);
            this.UpdateGrid(1);
        }

        /// <summary>
        /// Method to check updates in destination grid.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void DgDestination_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            this.UpdateDatagrid(e, 2);
            this.UpdateGrid(2);
        }

        /// <summary>
        /// Method to check if updated paths are valid.
        /// </summary>
        /// <param name="e">Object Sender.</param>
        /// <param name="mode">Mode 1 == exception, mode 2 == destination.</param>
        private void UpdateDatagrid(DataGridCellEditEndingEventArgs e, int mode)
        {
            FilePath oldValue = (FilePath)e.Row.Item;
            var newValue = e.EditingElement as System.Windows.Controls.TextBox;

            if (Directory.Exists(newValue.Text))
            {
                bool skip = false;

                if (mode == 1)
                {
                    // check if new entry is part of an exception
                    foreach (var item in this.exceptionPathList)
                    {
                        if (item.Path.ToLower().Equals(newValue.Text.ToLower()))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (!skip)
                    {
                        for (int i = 0; i < this.exceptionPathList.Count; i++)
                        {
                            if (this.exceptionPathList[i].Path.ToLower().Equals(oldValue.Path.ToLower()))
                            {
                                this.exceptionPathList[i].Path = newValue.Text;

                                LogHandler.Log("Exception list updated");

                                break;
                            }
                        }
                    }
                }
                else
                {
                    // checks if new entry was already entered earlier
                    foreach (var item in this.destinationPathList)
                    {
                        if (item.Path.ToLower().Equals(newValue.Text.ToLower()))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (!skip)
                    {
                        for (int i = 0; i < this.destinationPathList.Count; i++)
                        {
                            if (this.destinationPathList[i].Path.ToLower().Equals(oldValue.Path.ToLower()))
                            {
                                this.destinationPathList[i].Path = newValue.Text;

                                LogHandler.Log("Destination list updated");
                                break;
                            }
                        }
                    }
                }
            }

            // prevents program from crashing when enter is hit
            e.Cancel = true;
        }

        /// <summary>
        /// Method to start monitoring when program was started with a config file.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void Data_Sync_Loaded(object sender, RoutedEventArgs e)
        {
            if (loggingJobList.Count > 0)
            {
                btnSource.IsEnabled = false;
                cbRootOnly.IsEnabled = false;
                btnReset.IsEnabled = false;
                miImportConfig.IsEnabled = false;
                miManageLoggingFile.IsEnabled = false;
                cbBlock.IsEnabled = false;

                System.Windows.MessageBox.Show("Program started in automatic mode", "Automatic Mode");

                LogHandler.Log("Program started in automatic mode");

                this.StartMonitoring();
            }
        }

        /// <summary>
        /// Method to log whether root only is selected.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void CbRootOnly_Checked(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Root only checkbox checked");
        }

        /// <summary>
        /// Method to log whether root only is selected.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void CbRootOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Root only checkbox unchecked");
        }

        /// <summary>
        /// Method to check if synchronizing is finished when closing the program.
        /// </summary>
        /// <param name="sender">Object Sender.</param>
        /// <param name="e">Event args of the method.</param>
        private void Data_Sync_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool finished = true;

            foreach (var item in fileWatcherList)
            {
                if (!item.Finished)
                {
                    finished = false;
                    break;
                }
            }

            if (!finished)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Synchronization not finished yet. Are you sure you want to finish the application?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Method to open the jobs window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void MiShowJobs_Click(object sender, RoutedEventArgs e)
        {
            Jobs jobs = new Jobs();

            LogHandler.Log("Jobs window opened");

            jobs.Show();
        }

        /// <summary>
        /// Method when checkbox is checked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void CbBlock_Checked(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Checkbox for block monitoring checked");

            tbBlockSize.IsEnabled = true;
            tbFileSize.IsEnabled = true;
        }

        /// <summary>
        /// Method when checkbox is unchecked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void CbBlock_Unchecked(object sender, RoutedEventArgs e)
        {
            LogHandler.Log("Checkbox for block monitoring unchecked");

            tbBlockSize.IsEnabled = false;
            tbFileSize.IsEnabled = false;
        }

        /// <summary>
        /// Method when textbox is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void TbFileSize_Loaded(object sender, RoutedEventArgs e)
        {
            tbFileSize.Text = blockFileSize.ToString();
        }

        /// <summary>
        /// Method when textbox is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void TbBlockSize_Loaded(object sender, RoutedEventArgs e)
        {
            tbBlockSize.Text = blockSize.ToString();            
        }
    }
}