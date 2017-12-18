using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Web.SyncFileSystem
{
    [TableName("umbracoCacheFile")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class CacheFileDto
    {
        [Column("id")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [PrimaryKeyColumn(AutoIncrement = true, Name = "PK_umbracoCacheFile")]
        public int Id { get; set; }

        [Column("utcStamp")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime UtcStamp { get; set; }

        [Column("action")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Action { get; set; }

        [Column("path")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Path { get; set; }

        [Column("data")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public byte[] Data { get; set; }
    }
}



/*

CREATE TABLE [dbo].[umbracoCacheFile](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[utcStamp] [datetime] NOT NULL,
	[action] [int] NOT NULL,
	[path] [nvarchar](max) NOT NULL,
	[data] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_umbracoCacheFile] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

*/
