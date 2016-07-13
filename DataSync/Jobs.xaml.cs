// ----------------------------------------------------------------------- 
// <copyright file="Jobs.xaml.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for Jobs.
    /// </summary>
    public partial class Jobs : Window
    {
        /// <summary>
        /// Initializes static members of the <see cref="Jobs"/> class.
        /// </summary>
        static Jobs()
        {
            MyQueue = new List<MyQueue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Jobs"/> class.
        /// </summary>
        public Jobs()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the List with queue elements.
        /// </summary>
        /// <value>List contains elements for the job.</value>
        public static List<MyQueue> MyQueue { get; set; }

        /// <summary>
        /// Method to fill the grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<MyQueue> list = new List<DataSync.MyQueue>();

            for (int i = 0; i < 30 && i < MyQueue.Count; i++)
            {
                list.Add(MyQueue[i]);
            }

            dgJobs.ItemsSource = null;
            dgJobs.ItemsSource = list;
        }
    }
}