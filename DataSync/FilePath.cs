// ----------------------------------------------------------------------- 
// <copyright file="FilePath.cs" company="FHWN"> 
// Copyright (c) FHWN. All rights reserved. 
// </copyright> 
// <summary>This program synchronizes directories and monitores them.</summary> 
// <author>Wolfgang Ofner.</author> 
// -----------------------------------------------------------------------

namespace DataSync
{
     /// <summary>
    /// Class for grid content.
    /// </summary>
    public class FilePath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class.
        /// </summary>
        /// <param name="path">Contains a path.</param>
        public FilePath(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets or sets the value the value destination or exception path from grid.
        /// </summary>
        /// <value>The path of the destination or exception.</value>
        public string Path { get; set; }
    }
}
