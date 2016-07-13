// ----------------------------------------------------------------------- 
// <copyright file="FileWatcher.cs" company="FHWN"> 
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
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Class for monitoring directories.
    /// </summary>
    public class FileWatcher
    {
        /// <summary>
        /// Indicates whether block monitoring is active or not.
        /// </summary>
        private static bool blockMonitoring;

        /// <summary>
        /// File size when block monitoring will be used.
        /// </summary>
        private static double blockFileSize;

        /// <summary>
        /// Block size.
        /// </summary>
        private static int blockSize;

        /// <summary>
        /// File watcher.
        /// </summary>
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// File watcher for attributes.
        /// </summary>
        private FileSystemWatcher fileAttributeWatcher;

        /// <summary>
        /// String for the source.
        /// </summary>
        private string source;

        /// <summary>
        /// List for all the exceptions.
        /// </summary>
        private List<string> exception;

        /// <summary>
        /// List for all the destinations.
        /// </summary>
        private List<string> destination;

        /// <summary>
        /// Indicates whether OnChanged was already called.
        /// </summary>
        private bool onChangedAlreadyCalled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class.
        /// </summary>
        /// <param name="blockFileSizeParameter">File size for block monitoring.</param>
        /// <param name="blockSizeParameter">Block size.</param>
        public FileWatcher(double blockFileSizeParameter, int blockSizeParameter)
        {
            if (blockFileSizeParameter > 0 && blockSizeParameter > 0)
            {
                blockMonitoring = true;
                blockFileSize = blockFileSizeParameter;
                blockSize = blockSizeParameter;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class.
        /// </summary>
        /// <param name="loggingJob">Contains source, destination, root only and exceptions.</param>
        public FileWatcher(LoggingJob loggingJob)
        {
            this.fileWatcher = new FileSystemWatcher(loggingJob.Source);
            this.fileAttributeWatcher = new FileSystemWatcher(loggingJob.Source);

            this.fileWatcher.Created += new FileSystemEventHandler(this.OnCreated);
            this.fileWatcher.Deleted += new FileSystemEventHandler(this.OnDeleted);
            this.fileWatcher.Changed += new FileSystemEventHandler(this.OnChanged);
            this.fileWatcher.Renamed += new RenamedEventHandler(this.OnRenamed);
            this.fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            this.fileWatcher.IncludeSubdirectories = !loggingJob.RootOnly;
            this.fileWatcher.EnableRaisingEvents = true;

            // File watcher for attributes
            this.fileAttributeWatcher.Changed += new FileSystemEventHandler(this.OnChangedAttribute);
            this.fileAttributeWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.Security;
            this.fileAttributeWatcher.IncludeSubdirectories = !loggingJob.RootOnly;
            this.fileAttributeWatcher.EnableRaisingEvents = true;

            this.source = loggingJob.Source;
            this.exception = loggingJob.Exception;
            this.destination = loggingJob.Destination;
            this.Finished = false;

            LogHandler.Log("Filewatcher for " + loggingJob.Source + " " + "created");

            this.FirstSync(loggingJob);
        }

        /// <summary>
        /// Gets or sets a value indicating whether its finished or not.
        /// </summary>
        /// /// <value>Gets or sets the value of finished.</value>
        public bool Finished { get; set; }

        /// <summary>
        /// Copies source into destination.
        /// </summary>
        /// <param name="loggingJob">Contains source, destination, root only and exceptions.</param>
        private void FirstSync(LoggingJob loggingJob)
        {
            this.Finished = false;

            // Get the subdirectories and all files for the specified directory.
            DirectoryInfo directoryInfoSource = new DirectoryInfo(loggingJob.Source);
            List<string> allDirectories = new List<string>();
            List<FileInfo> allFiles = new List<FileInfo>();

            LogHandler.Log("First sync started");

            if (!directoryInfoSource.Exists)
            {
                MessageBox.Show("Source directory does not exist or could not be found: " + loggingJob.Source, "Source not found");
                LogHandler.Log("Source directory does not exist or could not be found: " + loggingJob.Source);
            }

            if (!loggingJob.RootOnly)
            {
                try
                {
                    allDirectories = Directory.GetDirectories(loggingJob.Source, "*", SearchOption.AllDirectories).ToList();
                    allFiles = directoryInfoSource.GetFiles("*", SearchOption.AllDirectories).ToList();
                }
                catch (Exception ex)
                {
                    LogHandler.Log("Error: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    allDirectories = Directory.GetDirectories(loggingJob.Source, "*", SearchOption.TopDirectoryOnly).ToList();
                    allFiles = directoryInfoSource.GetFiles("*", SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception ex)
                {
                    LogHandler.Log("Error: " + ex.Message);
                }
            }

            if (allDirectories.Count == 0 || allFiles.Count == 0)
            {
                LogHandler.Log("Error: Can't read directories or files.");
            }

            // remove exceptions from the directory list
            for (int i = 0; i < loggingJob.Exception.Count; i++)
            {
                for (int j = 0; j < allDirectories.Count; j++)
                {
                    // if directory equals exception or if it as subdirectory of the exception
                    if (allDirectories[j].Equals(loggingJob.Exception[i]) || allDirectories[j].Contains(loggingJob.Exception[i] + "\\"))
                    {
                        LogHandler.Log("Removed: " + allDirectories[j] + " from directory list");
                        allDirectories.Remove(allDirectories[j]);
                        j--;
                    }
                }
            }

            // remove exceptions from the file list
            for (int i = 0; i < loggingJob.Exception.Count; i++)
            {
                for (int j = 0; j < allFiles.Count; j++)
                {
                    // if file is in a subdirectory of an exception
                    if (allFiles[j].DirectoryName.Contains(loggingJob.Exception[i] + "\\"))
                    {
                        LogHandler.Log("Removed: " + allFiles[j] + " from file list");
                        allFiles.Remove(allFiles[j]);
                        j--;
                    }
                }
            }

            // loop for every destination
            for (int i = 0; i < loggingJob.Destination.Count; i++)
            {
                string[] allDirectioriesTemp = new string[allDirectories.Count];
                allDirectories.CopyTo(allDirectioriesTemp);

                // all directories
                for (int j = 0; j < allDirectioriesTemp.Length; j++)
                {
                    string help = allDirectioriesTemp[j];
                    allDirectioriesTemp[j] = allDirectioriesTemp[j].Replace(loggingJob.Source, loggingJob.Destination[i]);

                    Jobs.MyQueue.Add(new MyQueue(help, "Directory", allDirectioriesTemp[j], false));

                    LogHandler.Log("Path changed from: " + loggingJob.Source + " " + "to: " + loggingJob.Destination[i]);
                }

                // all files
                for (int j = 0; j < allFiles.Count; j++)
                {
                    string newPath = allFiles[j].DirectoryName;
                    newPath = newPath.Replace(loggingJob.Source, loggingJob.Destination[i]) + @"\";

                    Jobs.MyQueue.Add(new MyQueue(allFiles[j].DirectoryName, allFiles[j].Name, newPath, true));
                }
            }

            MyQueue.CopyJobs();

            this.Finished = true;
        }

        /// <summary>
        /// Fired if something is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileAttributes fileAttribute = new FileAttributes();

            try
            {
                fileAttribute = File.GetAttributes(e.FullPath);
            }
            catch (Exception ex)
            {
                LogHandler.Log("Error: " + ex.Message);
            }

            if (fileAttribute.HasFlag(FileAttributes.Directory))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(e.FullPath);
                List<DirectoryInfo> allDirectories = directoryInfo.GetDirectories("*", SearchOption.AllDirectories).ToList();
                List<FileInfo> allFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();

                foreach (var item in allDirectories)
                {
                    this.CopyDirectory(item.FullName);
                }

                foreach (var item in allFiles)
                {
                    this.CopyFile(item.FullName);
                }
            }
            else
            {
                this.CopyFile(e.FullPath);
            }
        }

        /// <summary>
        /// Fired if something is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            this.Finished = false;

            FileAttributes fileAttribute = new FileAttributes();
            string newPath = e.FullPath;

            newPath = newPath.Replace(this.source, this.destination[0]);

            try
            {
                fileAttribute = File.GetAttributes(newPath);
            }
            catch (Exception ex)
            {
                LogHandler.Log("Error: " + ex.Message);
            }

            if (fileAttribute.HasFlag(FileAttributes.Directory))
            {
                // Loop for all destination
                for (int i = 0; i < this.destination.Count; i++)
                {
                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(newPath);
                        List<FileInfo> allFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();
                        List<DirectoryInfo> allDirectories = directoryInfo.GetDirectories("*", SearchOption.AllDirectories).ToList();

                        foreach (var item in allFiles)
                        {
                            File.SetAttributes(item.FullName, FileAttributes.Normal);
                            File.Delete(item.FullName);
                        }

                        foreach (var item in allDirectories)
                        {
                            Directory.Delete(item.FullName);
                        }

                        Directory.Delete(newPath);

                        LogHandler.Log("Directory deleted: " + newPath);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Log("Error: " + ex.Message);
                    }

                    // Replace current destination with next destination, if last destination do nothing
                    if (i != this.destination.Count - 1)
                    {
                        newPath = newPath.Replace(this.destination[i], this.destination[i + 1]);
                    }
                }
            }
            else
            {
                // Loop for all destination
                for (int i = 0; i < this.destination.Count; i++)
                {
                    try
                    {
                        File.SetAttributes(newPath, FileAttributes.Normal);
                        File.Delete(newPath);

                        LogHandler.Log("File deleted: " + newPath);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Log("Error: " + ex.Message);
                    }

                    // Replace current destination with next destination, if last destination do nothing
                    if (i != this.destination.Count - 1)
                    {
                        newPath = newPath.Replace(this.destination[i], this.destination[i + 1]);
                    }
                }
            }

            this.Finished = true;
        }

        /// <summary>
        /// Fired if something is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileAttributes fileAttribute = new FileAttributes();

            // Prevents multiple OnChanged and OnChangedAttribute calls.
            this.onChangedAlreadyCalled = true;
            this.fileWatcher.EnableRaisingEvents = false;
            this.fileWatcher.EnableRaisingEvents = true;

            // onChanged is also called on a directory, if a directory is created inside of it 
            try
            {
                fileAttribute = File.GetAttributes(e.FullPath);
            }
            catch (Exception ex)
            {
                LogHandler.Log("Error: " + ex.Message);
            }

            // only execute on files
            if (!fileAttribute.HasFlag(FileAttributes.Directory))
            {
                this.CopyFile(e.FullPath);
            }

            this.onChangedAlreadyCalled = false;
        }

        /// <summary>
        /// Fired if an attribute is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void OnChangedAttribute(object sender, FileSystemEventArgs e)
        {
            FileAttributes fileAttribute = new FileAttributes();
            this.fileAttributeWatcher.EnableRaisingEvents = false;

            try
            {
                fileAttribute = File.GetAttributes(e.FullPath);
            }
            catch (Exception ex)
            {
                LogHandler.Log("Error: " + ex.Message);
            }

            // Only if OnChanged wasn't called at the same time.
            if (!this.onChangedAlreadyCalled)
            {
                if (fileAttribute.HasFlag(FileAttributes.Directory))
                {
                    this.ChangeDirectoryAttributes(e.FullPath);
                }
                else
                {
                    this.CopyFile(e.FullPath, false);
                }                
            }

            this.fileAttributeWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Method to change attributes of folder.
        /// </summary>
        /// <param name="path">Path of the folder.</param>
        private void ChangeDirectoryAttributes(string path)
        {
            this.Finished = false;
            bool skip = false;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            // remove exceptions from the directory list
            for (int i = 0; i < this.exception.Count; i++)
            {
                // if directory equals exception or if it as subdirectory of the exception
                if (directoryInfo.FullName.Equals(this.exception[i]) || directoryInfo.FullName.Contains(this.exception[i] + "\\"))
                {
                    skip = true;
                    LogHandler.Log("Directory copy skipped: " + directoryInfo.FullName + " " + "is part of " + this.exception[i]);
                    break;
                }
            }

            if (!skip)
            {
                for (int i = 0; i < this.destination.Count; i++)
                {
                    string newPath = directoryInfo.FullName;
                    newPath = newPath.Replace(this.source, this.destination[i]) + @"\";

                    if (Directory.Exists(newPath))
                    {
                        try
                        {
                            DirectoryInfo directoryInfoDestination = new DirectoryInfo(newPath);
                            directoryInfoDestination.Attributes = directoryInfo.Attributes;

                            LogHandler.Log("Directory attributes changed: " + newPath);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                }
            }
            
            this.Finished = true;
        }

        /// <summary>
        /// Fired if something is renamed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            this.Finished = false;

            LogHandler.Log("OnRenamed event triggered");

            FileAttributes fileAttribute = File.GetAttributes(e.FullPath);

            if (fileAttribute.HasFlag(FileAttributes.Directory))
            {
                this.RenameDirectory(e.FullPath, e.OldFullPath);
            }
            else
            {
                this.RenameFile(e.FullPath, e.OldFullPath);
            }
        }

        /// <summary>
        /// Method to copy files.
        /// </summary>
        /// <param name="filePath">Path of file for copying.</param>
        /// <param name="blockEnabled">Indicates if block monitoring is enabled. Deactivated for attribute sync.</param>
        private void CopyFile(string filePath, bool blockEnabled = true)
        {
            this.Finished = false;
            bool skip = false;
            FileInfo fileInfo = new FileInfo(filePath);

            for (int i = 0; i < this.exception.Count; i++)
            {
                if (fileInfo.DirectoryName.Contains(this.exception[i] + "\\"))
                {
                    skip = true;

                    LogHandler.Log("File copy skipped: " + fileInfo.DirectoryName + " " + "is part of " + this.exception[i]);
                    break;
                }
            }

            if (!skip)
            {
                for (int i = 0; i < this.destination.Count; i++)
                {
                    string newPath = fileInfo.DirectoryName;
                    newPath = newPath.Replace(this.source, this.destination[i]) + @"\";

                    if (blockMonitoring && blockEnabled && fileInfo.Length > blockFileSize)
                    {
                        if (!File.Exists(newPath + fileInfo.Name))
                        {
                            try
                            {
                                File.Copy(fileInfo.FullName, newPath + fileInfo.Name, true);

                                LogHandler.Log("File copied from: " + fileInfo.FullName + " " + "to: " + newPath + fileInfo.Name);
                            }
                            catch (Exception ex)
                            {
                                LogHandler.Log("Error: " + ex.Message);
                            }
                        }
                        else
                        {
                            LogHandler.Log("Using block copying");

                            int block = 0;
                            string fileSource = fileInfo.FullName;
                            string fileDestination = newPath + fileInfo.Name;

                            byte[] sourceFile = File.ReadAllBytes(fileSource);
                            byte[] destinationFile = File.ReadAllBytes(fileDestination);

                            for (int j = 0; j < sourceFile.Length && j < destinationFile.Length; j++)
                            {
                                if (sourceFile[j] != destinationFile[j])
                                {
                                    block = j / blockSize;

                                    this.ReplaceData(sourceFile, block, blockSize, fileDestination);
                                    j = (1 + block) * blockSize;
                                    j--;
                                }
                            }

                            if (destinationFile.Length < sourceFile.Length)
                            {
                                int currentBlock = destinationFile.Length / blockSize;
                                int remainingBytes = sourceFile.Length - (currentBlock * blockSize);

                                this.AddEndOfFile(fileDestination, sourceFile, currentBlock, blockSize);

                                LogHandler.Log("File copied from: " + fileInfo.FullName + " " + "to: " + newPath + fileInfo.Name + " " + "using block copying");
                            }

                            if (destinationFile.Length > sourceFile.Length)
                            {
                                this.DeleteEndOfFile(fileDestination, sourceFile.Length);

                                LogHandler.Log("File copied from: " + fileInfo.FullName + " " + "to: " + newPath + fileInfo.Name + " " + "using block copying");
                            }
                            else if (destinationFile.Length == 0)
                            {
                                // If file is empty.
                                try
                                {
                                    File.Copy(fileInfo.FullName, newPath + fileInfo.Name, true);

                                    LogHandler.Log("File copied from: " + fileInfo.FullName + " " + "to: " + newPath + fileInfo.Name);
                                }
                                catch (Exception ex)
                                {
                                    LogHandler.Log("Error: " + ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        // No block monitoring.
                        if (File.Exists(newPath + fileInfo.Name))
                        {
                            try
                            {
                                File.SetAttributes(newPath + fileInfo.Name, FileAttributes.Normal);
                                File.Delete(newPath + fileInfo.Name);

                                LogHandler.Log("File deleted: " + newPath + fileInfo.Name);
                            }
                            catch (Exception ex)
                            {
                                LogHandler.Log("Error: " + ex.Message);
                            }
                        }

                        try
                        {
                            File.Copy(fileInfo.FullName, newPath + fileInfo.Name, true);

                            LogHandler.Log("File copied from: " + fileInfo.FullName + " " + "to: " + newPath + fileInfo.Name);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                }
            }

            this.Finished = true;
        }

        /// <summary>
        /// Method replaces binary blocks of a file.
        /// </summary>
        /// <param name="sourceFile">Contains binary of the source.</param>
        /// <param name="block">Indicates current block.</param>
        /// <param name="size">Block size.</param>
        /// <param name="filePathDestination">File path of the destination file.</param>
        private void ReplaceData(byte[] sourceFile, int block, int size, string filePathDestination)
        {
            int offset = size;

            using (BinaryWriter fileWriter = new BinaryWriter(File.Open(filePathDestination, FileMode.Open)))
            {
                try
                {
                    if (((block * size) + size) > sourceFile.Length)
                    {
                        offset = sourceFile.Length - (block * size);
                    }

                    // set the offset
                    fileWriter.BaseStream.Position = block * size;
                    fileWriter.Write(sourceFile, block * size, offset);
                }
                catch (Exception ex)
                {
                    LogHandler.Log("Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Method to add the rest of the source file if it is longer than the destination file.
        /// </summary>
        /// <param name="filePathDestination">Path of the destination file.</param>
        /// <param name="sourceFile">Byte array of the source file.</param>
        /// <param name="block">Current block.</param>
        /// <param name="blockSize">Size of a block.</param>
        private void AddEndOfFile(string filePathDestination, byte[] sourceFile, int block, int blockSize)
        {
            int offset = sourceFile.Length - (block * blockSize);

            using (BinaryWriter fileWriter = new BinaryWriter(File.Open(filePathDestination, FileMode.Open)))
            {
                try
                {
                    // set the offset
                    fileWriter.BaseStream.Position = block * blockSize;
                    fileWriter.Write(sourceFile, block * blockSize, offset);

                    LogHandler.Log("End of file added");
                }
                catch (Exception ex)
                {
                    LogHandler.Log("Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Method to delete end of file if destination file is larger than source file.
        /// </summary>
        /// <param name="filePath">File path destination file.</param>
        /// <param name="fileSize">File size destination file.</param>
        private void DeleteEndOfFile(string filePath, int fileSize)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                fs.SetLength(fileSize);
                fs.Close();

                LogHandler.Log("End of file deleted");
            }
            catch (Exception ex)
            {
                LogHandler.Log("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Method to copy directories.
        /// </summary>
        /// <param name="path">Path of directory for copying.</param>
        private void CopyDirectory(string path)
        {
            this.Finished = false;
            bool skip = false;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            // remove exceptions from the directory list
            for (int i = 0; i < this.exception.Count; i++)
            {
                // if directory equals exception or if it as subdirectory of the exception
                if (directoryInfo.FullName.Equals(this.exception[i]) || directoryInfo.FullName.Contains(this.exception[i] + "\\"))
                {
                    skip = true;
                    LogHandler.Log("Directory copy skipped: " + directoryInfo.FullName + " " + "is part of " + this.exception[i]);
                    break;
                }
            }

            if (!skip)
            {
                for (int i = 0; i < this.destination.Count; i++)
                {
                    string newPath = directoryInfo.FullName;
                    newPath = newPath.Replace(this.source, this.destination[i]) + @"\";

                    if (!Directory.Exists(newPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(newPath);

                            LogHandler.Log("Directory created: " + newPath);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                }
            }

            this.Finished = true;
        }

        /// <summary>
        /// Method to rename a file.
        /// </summary>
        /// <param name="newName">New file name.</param>
        /// <param name="oldName">Old file name.</param>
        private void RenameFile(string newName, string oldName)
        {
            this.Finished = false;
            bool skip = false;
            FileInfo fileInfoNew = new FileInfo(newName);
            FileInfo fileInfoOld = new FileInfo(oldName);

            for (int i = 0; i < this.exception.Count; i++)
            {
                if (fileInfoNew.DirectoryName.Contains(this.exception[i] + "\\"))
                {
                    skip = true;
                    LogHandler.Log("File copy skipped: " + fileInfoNew.DirectoryName + " " + "is part of " + this.exception[i]);

                    break;
                }
            }

            if (!skip)
            {
                for (int i = 0; i < this.destination.Count; i++)
                {
                    string newPath = fileInfoNew.DirectoryName;
                    string oldPath = fileInfoOld.DirectoryName;
                    newPath = newPath.Replace(this.source, this.destination[i]) + @"\";
                    oldPath = oldPath.Replace(this.source, this.destination[i]) + @"\";

                    if (File.Exists(newPath + fileInfoNew.Name))
                    {
                        try
                        {
                            File.SetAttributes(newPath + fileInfoNew.Name, FileAttributes.Normal);
                            File.Delete(newPath + fileInfoNew.Name);

                            LogHandler.Log("File deleted: " + newPath + fileInfoNew.Name);

                            File.Move(oldPath + fileInfoOld.Name, newPath + fileInfoNew.Name);

                            LogHandler.Log("File renamed from: " + oldPath + fileInfoOld.Name + " " + "to: " + newPath + fileInfoNew.Name);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Move(oldPath + fileInfoOld.Name, newPath + fileInfoNew.Name);

                            LogHandler.Log("File renamed from: " + oldPath + fileInfoOld.Name + " " + "to: " + newPath + fileInfoNew.Name);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                }
            }

            this.Finished = true;
        }

        /// <summary>
        /// Method to rename a directory.
        /// </summary>
        /// <param name="newName">New directory name.</param>
        /// <param name="oldName">Old directory name.</param>
        private void RenameDirectory(string newName, string oldName)
        {
            this.Finished = false;
            bool skip = false;
            DirectoryInfo directoryInfoNew = new DirectoryInfo(newName);
            DirectoryInfo directoryInfoOld = new DirectoryInfo(oldName);

            // remove exceptions from the directory list
            for (int i = 0; i < this.exception.Count; i++)
            {
                // if directory equals exception or if it as subdirectory of the exception
                if (directoryInfoNew.FullName.Equals(this.exception[i]) || directoryInfoNew.FullName.Contains(this.exception[i] + "\\"))
                {
                    skip = true;
                    LogHandler.Log("Directory copy skipped: " + directoryInfoNew.FullName + " " + "is part of " + this.exception[i]);
                    break;
                }
            }

            if (!skip)
            {
                for (int i = 0; i < this.destination.Count; i++)
                {
                    string newPath = directoryInfoNew.FullName;
                    string oldPath = directoryInfoOld.FullName;
                    newPath = newPath.Replace(this.source, this.destination[i]) + @"\";
                    oldPath = oldPath.Replace(this.source, this.destination[i]) + @"\";

                    if (!Directory.Exists(newPath))
                    {
                        try
                        {
                            Directory.Move(oldPath, newPath);

                            LogHandler.Log("Directory renamed from: " + oldPath + " " + "to: " + newPath);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                    }
                }
            }

            this.Finished = true;
        }
    }
}