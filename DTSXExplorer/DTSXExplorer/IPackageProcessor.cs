using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTSXExplorer
{
    internal interface IPackageProcessor
    {
        /// <summary>
        /// Export a single DTSX file.
        /// </summary>
        /// <param name="packagePath">The path of DTSX file.</param>
        /// <param name="destinationFile">The path of file containing the exported file.</param>
        void Export(string packagePath, string destinationFile);

        /// <summary>
        /// Export multiple DTSX files.
        /// </summary>
        /// <param name="packagePathsList">The path of folder containing the DTSX files.</param>
        /// <param name="destinationFolder">The path of file containing the exported files.</param>
        void ExportBatch(string packagePathsList, string destinationFolder);
    }
}
