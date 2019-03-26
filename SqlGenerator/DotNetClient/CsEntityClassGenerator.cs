﻿using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.DotNetClient
{
    public class CsEntityClassGenerator : GeneratorBase
    {
        private readonly CsEntityClassGeneratorSettings _settings;

        public CsEntityClassGenerator(GeneratorSettings generatorSettings, TSqlObject table)
            : base(generatorSettings, table)
        {
            _settings = TableSettings?.CsEntitySettings ?? GeneratorSettings.GlobalSettings.CsEntitySettings;

            //todo to be implemented
            //this.WithStandardDecorator = withStandarDecorator ?? this.WithStandardDecorator;
        }

        public override string Generate()
        {
            var allColumns = Table.GetAllColumns().Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            var memberDeclarations = String.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var memberName = TSqlModelHelper.PascalCase(col.Name.Parts[2]);
                var colDataType = col.GetColumnSqlDataType(false);
                var isNullable = col.IsColumnNullable();
                var memberType = TSqlModelHelper.GetDotNetDataType(colDataType, isNullable);

                var decorators = "";
                if (memberType == "string")
                {
                    var colLen = col.GetProperty<int>(Column.Length);
                    if (colLen > 0)
                    {
                        decorators += $"[StringLength({colLen})]"
                            + Environment.NewLine + "        ";
                    }
                }
                if (!isNullable)
                {
                    decorators += $"[Required]"
                            + Environment.NewLine + "        ";
                }


                return $"{decorators}public {memberType} {memberName} {{ get; set; }}";
            }));

            string output =
$@" 
-- =================================================================
-- Author: {GeneratorSettings.AuthorName}
-- Description:	Entity class for the table {Table.Name} 
-- =================================================================

namespace { _settings.Namespace } {{
  
    public class { TSqlModelHelper.PascalCase(Table.Name.Parts[1]) } : ICloneable
    {{ 
        
        { memberDeclarations }


        public object Clone()
        {{
            return this.MemberwiseClone();
        }}

    }}
}}

";

            return output;
        }


    }
}


