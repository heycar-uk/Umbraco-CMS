﻿using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("6.2.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class AdditionalIndexesAndKeys : MigrationBase
    {
        public override void Up()
        {

            var dbIndexes = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //do not create any indexes if they already exist in the database

            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodeTrashed")) == false)
            {
                Create.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode").OnColumn("trashed").Ascending().WithOptions().NonClustered();    
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsContentVersion_ContentId")) == false)
            {
                Create.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion").OnColumn("ContentId").Ascending().WithOptions().NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDocument_published")) == false)
            {
                Create.Index("IX_cmsDocument_published").OnTable("cmsDocument").OnColumn("published").Ascending().WithOptions().NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsDocument_newest")) == false)
            {
                Create.Index("IX_cmsDocument_newest").OnTable("cmsDocument").OnColumn("newest").Ascending().WithOptions().NonClustered();
            }

            //we want to drop the umbracoUserLogins_Index index since it is named incorrectly and then re-create it so 
            // it follows the standard naming convention
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("umbracoUserLogins_Index")))
            {
                Delete.Index("umbracoUserLogins_Index").OnTable("umbracoUserLogins");                
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoUserLogins_Index")) == false)
            {
                Create.Index("IX_umbracoUserLogins_Index").OnTable("umbracoUserLogins").OnColumn("contextID").Ascending().WithOptions().Clustered();
            }
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode");
            Delete.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion");
            Delete.Index("IX_cmsDocument_published").OnTable("cmsDocument");
            Delete.Index("IX_cmsDocument_newest").OnTable("cmsDocument");
        }
    }
}