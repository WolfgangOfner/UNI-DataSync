// ----------------------------------------------------------------------- 
// <copyright file="LoggingJob.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner..</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class with all the information for the monitoring.
    /// </summary>
    public class LoggingJob
    {      
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingJob"/> class.
        /// </summary>
        /// <param name="source">Source path.</param>
        /// <param name="rootOnly">Boolean for root only.</param>
        /// <param name="exception">Exception paths.</param>
        /// <param name="destination">Destination paths.</param>
        public LoggingJob(string source, bool rootOnly, List<string> exception, List<string> destination)
        {            
            this.Source = source;
            this.Exception = exception.ToList();
            this.Destination = destination.ToList();
            this.RootOnly = rootOnly;            
        }

        /// <summary>
        /// Gets or sets the value of the source.
        /// </summary>
        /// <value>The path of the source.</value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the value of the exceptions.
        /// </summary>
        /// <value>The path of the exceptions.</value>
        public List<string> Exception { get; set; }

        /// <summary>
        /// Gets or sets the value of the destination.
        /// </summary>
        /// <value>The path of the destinations.</value>
        public List<string> Destination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether root only is true.
        /// </summary>
        /// <value>The path of the destination or exception.</value>
        public bool RootOnly { get; set; }
    }
}