// <copyright file="EditorLanguage.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Identifies the language used for the query editor.
    /// </summary>
    public class EditorLanguage
    {
        private EditorLanguage(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Value of the language.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Converts an <see cref="EditorLanguage"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="editorLanguage">Value to convert.</param>
        public static implicit operator string(EditorLanguage editorLanguage) => editorLanguage.Value;

        /// <summary>
        /// Converts a <see cref="string"/> to an <see cref="EditorLanguage"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static implicit operator EditorLanguage(string value) => new(value);

        public const string Abap = "abap";

        public const string Apex = "apex";

        public const string AzCli = "azcli";

        public const string Bat = "bat";

        public const string Bicep = "bicep";

        public const string Cameligo = "cameligo";

        public const string Clojure = "clojure";

        public const string Coffee = "coffee";

        public const string Cpp = "cpp";

        public const string CSharp = "csharp";

        public const string Csp = "csp";

        public const string Css = "css";

        public const string Cypher = "cypher";

        public const string Dart = "dart";

        public const string Dockerfile = "dockerfile";

        public const string Ecl = "ecl";

        public const string Elixir = "elixir";

        public const string Flow9 = "flow9";

        public const string Freemarker2 = "freemarker2";

        public const string FSharp = "fsharp";

        public const string Go = "go";

        public const string GraphQl = "graphql";

        public const string Handlebars = "handlebars";

        public const string Hcl = "hcl";

        public const string Html = "html";

        public const string Ini = "ini";

        public const string Java = "java";

        public const string JavaScript = "javascript";

        public const string Julia = "julia";

        public const string Kotlin = "kotlin";

        public const string Less = "less";

        public const string Lexon = "lexon";

        public const string Liquid = "liquid";

        public const string Lua = "lua";

        public const string M3 = "m3";

        public const string Markdown = "markdown";

        public const string Mips = "mips";

        public const string Msdax = "msdax";

        public const string MySql = "mysql";

        public const string ObjectiveC = "objective-c";

        public const string Pascal = "pascal";

        public const string Pascaligo = "pascaligo";

        public const string Perl = "perl";

        public const string PgSql = "pgsql";

        public const string Php = "php";

        public const string Pla = "pla";

        public const string Postiats = "postiats";

        public const string PowerQuery = "powerquery";

        public const string PowerShell = "powershell";

        public const string Protobuf = "protobuf";

        public const string Pug = "pug";

        public const string Python = "python";

        public const string QSharp = "qsharp";

        public const string R = "r";

        public const string Razor = "razor";

        public const string Redis = "redis";

        public const string Redshift = "redshift";

        public const string RestructuredText = "restructuredtext";

        public const string Ruby = "ruby";

        public const string Rust = "rust";

        public const string Sb = "sb";

        public const string Scala = "scala";

        public const string Scheme = "scheme";

        public const string Scss = "scss";

        public const string Shell = "shell";

        public const string Solidity = "solidity";

        public const string Sophia = "sophia";

        public const string Sparql = "sparql";

        public const string Sql = "sql";

        public const string St = "st";

        public const string Swift = "swift";

        public const string SystemVerilog = "systemverilog";

        public const string Tcl = "tcl";

        public const string Twig = "twig";

        public const string TypeScript = "typescript";

        public const string Vb = "vb";

        public const string Wgsl = "wgsl";

        public const string Xml = "xml";

        public const string Yaml = "yaml";

        public override string ToString()
        {
            return Value;
        }
    }
}
