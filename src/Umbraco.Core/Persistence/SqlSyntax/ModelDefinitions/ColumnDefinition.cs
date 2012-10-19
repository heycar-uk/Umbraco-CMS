﻿using System;

namespace Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions
{
    public class ColumnDefinition
    {
        public string ColumnName { get; set; }

        public Type PropertyType { get; set; }
        public string DbType { get; set; }
        public int? DbTypeLength { get; set; }
        
        public bool IsNullable { get; set; }
        
        public bool IsPrimaryKey { get; set; }
        public bool IsPrimaryKeyIdentityColumn { get; set; }
        public bool IsPrimaryKeyClustered { get; set; }
        public string PrimaryKeyName { get; set; }
        public string PrimaryKeyColumns { get; set; }

        public string ConstraintName { get; set; }
        public string ConstraintDefaultValue { get; set; }
        public bool HasConstraint
        {
            get { return !string.IsNullOrEmpty(ConstraintName) || !string.IsNullOrEmpty(ConstraintDefaultValue); }
        }
    }
}