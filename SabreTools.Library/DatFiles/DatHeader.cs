using System;
using SabreTools.Library.Data;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents all possible DAT header information
    /// </summary>
    public class DatHeader : ICloneable
    {
        #region Publicly facing variables

        // Data common to most DAT types
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RootDir { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Date { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public string Homepage { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
        public string Header { get; set; }
        public string Type { get; set; } // Generally only used for SuperDAT
        public ForceMerging ForceMerging { get; set; }
        public ForceNodump ForceNodump { get; set; }
        public ForcePacking ForcePacking { get; set; }
        public DatFormat DatFormat { get; set; }
        public bool[] ExcludeFields { get; set; } = new bool[Enum.GetNames(typeof(Field)).Length];
        public bool OneRom { get; set; }
        public bool KeepEmptyGames { get; set; }
        public bool SceneDateStrip { get; set; }
        public DedupeType DedupeRoms { get; set; }
        public Hash StripHash { get; private set; }

        // Data specific to the Miss DAT type
        public bool UseRomName { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public bool Quotes { get; set; }
        public string ReplaceExtension { get; set; }
        public string AddExtension { get; set; }
        public bool RemoveExtension { get; set; }
        public bool GameName { get; set; }
        public bool Romba { get; set; }

        #endregion

        #region Instance Methods

        #region Cloning Methods

        /// <summary>
        /// Clone the current header
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new DatHeader()
            {
                FileName = this.FileName,
                Name = this.Name,
                Description = this.Description,
                RootDir = this.RootDir,
                Category = this.Category,
                Version = this.Version,
                Date = this.Date,
                Author = this.Author,
                Email = this.Email,
                Homepage = this.Homepage,
                Url = this.Url,
                Comment = this.Comment,
                Header = this.Header,
                Type = this.Type,
                ForceMerging = this.ForceMerging,
                ForceNodump = this.ForceNodump,
                ForcePacking = this.ForcePacking,
                DatFormat = this.DatFormat,
                ExcludeFields = this.ExcludeFields,
                OneRom = this.OneRom,
                KeepEmptyGames = this.KeepEmptyGames,
                SceneDateStrip = this.SceneDateStrip,
                DedupeRoms = this.DedupeRoms,
                StripHash = this.StripHash,

                UseRomName = this.UseRomName,
                Prefix = this.Prefix,
                Postfix = this.Postfix,
                Quotes = this.Quotes,
                ReplaceExtension = this.ReplaceExtension,
                AddExtension = this.AddExtension,
                RemoveExtension = this.RemoveExtension,
                GameName = this.GameName,
                Romba = this.Romba,
            };
        }

        #endregion

        #endregion // Instance Methods
    }
}
