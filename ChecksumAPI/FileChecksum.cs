using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChecksumAPI
{
    public class FileChecksum
    {
        public string FileUrl { get; set; }
        public byte OffsetPercent { get; set; }
        public string Algorithm { get; set; }
        public string Checksum { get; set; }

        #region configurations
        public class Configuration : IEntityTypeConfiguration<FileChecksum>
        {
            public void Configure(EntityTypeBuilder<FileChecksum> builder)
            {
                builder.ToTable(nameof(FileChecksum).Pluralize());
                builder.HasKey(fc => new { fc.FileUrl, fc.OffsetPercent });
            }
        }
        #endregion
    }
}