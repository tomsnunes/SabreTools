using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    /// TODO: Can this be overhauled to have all types write like DatFiles?
    public abstract class BaseReport
    {
        #region Protected instance variables

        /// <summary>
        /// DatFile to write statistics for
        /// </summary>
        protected DatFile _datFile;

        /// <summary>
        /// Whether to include baddump counts
        /// </summary>
        protected bool _baddumpCol = false;

        /// <summary>
        /// Whether to include nodump counts
        /// </summary>
        protected bool _nodumpCol = false;

        #region Publicly facing variables

        /// <summary>
        /// External name of the Report
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Read or write format
        /// </summary>
        public StatReportFormat ReportFormat { get; set; }

        #endregion

        /// <summary>
        ///  Create a new, empty BaseReport object
        /// </summary>
        public BaseReport()
        {
        }

        /// <summary>
        /// Create a new BaseReport from an existing one
        /// </summary>
        /// <param name="baseReport">BaseReport to get the values from</param>
        public BaseReport(BaseReport baseReport)
        {
            this._datFile = baseReport._datFile;
            this._nodumpCol = baseReport._nodumpCol;
            this._baddumpCol = baseReport._baddumpCol;
            this.FileName = baseReport.FileName;
        }

        /// <summary>
        /// Create a new report from the input DatFile and columns
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="filename">Filename to use for output</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public BaseReport(DatFile datfile, string filename, bool baddumpCol = false, bool nodumpCol = false)
        {
            _datFile = datfile;
            FileName = filename;
            _baddumpCol = baddumpCol;
            _nodumpCol = nodumpCol;
        }

        /// <summary>
        /// Replace the DatFile that is being output
        /// </summary>
        /// <param name="datfile"></param>
        public void ReplaceDatFile(DatFile datfile)
        {
            _datFile = datfile;
        }

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outDir">Set the output directory (default current directory)</param>
        /// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public bool Write(string outDir = null, bool overwrite = true)
        {
            // Ensure the output directory is set and created
            outDir = Utilities.EnsureOutputDirectory(outDir, create: true);

            // If the DAT has no output format, default to textfile
            if (ReportFormat == StatReportFormat.None)
            {
                Globals.Logger.Verbose("No report format defined, defaulting to textfile");
                ReportFormat = StatReportFormat.Textfile;
            }

            // Get the outfile names
            Dictionary<StatReportFormat, string> outfiles = CreateOutfileNames(outDir, overwrite);

            try
            {
                // Write out all required formats
                Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, reportFormat =>
                {
                    string outfile = outfiles[reportFormat];
                    try
                    {
                        Utilities.GetBaseReport(reportFormat, this)?.WriteToFile(outfile);
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error($"Datfile {outfile} could not be written out: {ex}");
                    }

                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create and open an output file for writing direct
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        public virtual bool WriteToFile(string outfile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the proper extension for the stat output format
        /// </summary>
        /// <param name="outDir">Output path to use</param>
        /// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        private Dictionary<StatReportFormat, string> CreateOutfileNames(string outDir, bool overwrite = true)
        {
            Dictionary<StatReportFormat, string> output = new Dictionary<StatReportFormat, string>();

            // First try to create the output directory if we need to
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // For each output format, get the appropriate stream writer
            if ((ReportFormat & StatReportFormat.Textfile) != 0)
                output.Add(StatReportFormat.Textfile, CreateOutfileNamesHelper(outDir, ".txt", overwrite));

            if ((ReportFormat & StatReportFormat.CSV) != 0)
                output.Add(StatReportFormat.CSV, CreateOutfileNamesHelper(outDir, ".csv", overwrite));

            if ((ReportFormat & StatReportFormat.HTML) != 0)
                output.Add(StatReportFormat.HTML, CreateOutfileNamesHelper(outDir, ".html", overwrite));

            if ((ReportFormat & StatReportFormat.SSV) != 0)
                output.Add(StatReportFormat.SSV, CreateOutfileNamesHelper(outDir, ".ssv", overwrite));

            if ((ReportFormat & StatReportFormat.TSV) != 0)
                output.Add(StatReportFormat.TSV, CreateOutfileNamesHelper(outDir, ".tsv", overwrite));

            return output;
        }

        /// <summary>
        /// Help generating the outfile name
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private string CreateOutfileNamesHelper(string outDir, string extension, bool overwrite)
        {
            string outfile = $"{outDir}{FileName}{extension}";
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{FileName}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }
    }
}
