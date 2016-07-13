// ----------------------------------------------------------------------- 
// <copyright file="MyQueue.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System;
    using System.IO;

    /// <summary>
    /// Class for queue jobs.
    /// </summary>
    public class MyQueue
    {
        /// <summary>
        /// Locker object.
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="MyQueue"/> class.
        /// </summary>
        /// <param name="source">Path of the source.</param>
        /// <param name="name">File name.</param>
        /// <param name="destination">Path of the destination.</param>
        /// <param name="isFile">Boolean whether it is a file.</param>
        public MyQueue(string source, string name, string destination, bool isFile)
        {
            this.Source = source;
            this.Name = name;
            this.Destination = destination;
            this.Processing = false;
            this.IsFile = isFile;
        }

        /// <summary>
        /// Gets or sets the value the value the source path.
        /// </summary>
        /// <value>The path of the source.</value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the value the value of the name.
        /// </summary>
        /// <value>The name of the file.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value the value of the destination path.
        /// </summary>
        /// <value>The path of the destination.</value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the job is processing right now.
        /// </summary>
        /// /// <value>Gets or sets the value of is processing.</value>
        public bool Processing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is a file.
        /// </summary>
        /// /// <value>Gets or sets the value of is a file.</value>
        public bool IsFile { get; set; }

        /// <summary>
        /// Method for copying files and directories.
        /// </summary>
        public static void CopyJobs()
        {
            lock (locker)
            {
                for (int i = 0; i < Jobs.MyQueue.Count; i++)
                {
                    var item = Jobs.MyQueue[i];
                
                    if (item.IsFile)
                    {
                        if (File.Exists(item.Destination + item.Name))
                        {
                            try
                            {
                                File.SetAttributes(item.Destination + item.Name, FileAttributes.Normal);
                                File.Delete(item.Destination + item.Name);

                                LogHandler.Log("File deleted: " + item.Destination + item.Name);
                            }
                            catch (Exception ex)
                            {
                                LogHandler.Log("Error: " + ex.Message);
                            }
                        }

                        try
                        {
                            item.Processing = true;

                            File.Copy(item.Source + "\\" + item.Name, item.Destination + item.Name, true);

                            LogHandler.Log("File copied from: " + item.Source + "\\" + item.Name + " " + "to: " + item.Destination + item.Name);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.Log("Error: " + ex.Message);
                        }
                        finally
                        {
                            item.Processing = false;
                            Jobs.MyQueue.Remove(item);
                            i--;
                        }
                    }
                    else
                    {
                        item.Processing = true;

                        if (!Directory.Exists(item.Destination))
                        {
                            Directory.CreateDirectory(item.Destination);

                            LogHandler.Log("Directory created: " + item.Destination);
                        }

                        item.Processing = false;
                        Jobs.MyQueue.Remove(item);
                        i--;
                    }
                }
            }
        }       
    }
}