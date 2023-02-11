﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.CodeAnalysis
{
    internal readonly struct GeneratedSource : IEquatable<GeneratedSource>
    {
        public readonly String Text;
        public readonly String HintName;

        public GeneratedSource(String source, String fileName, Boolean disableWarnings = true) : this(CSharpSyntaxTree.ParseText(source).GetRoot(), fileName, disableWarnings)
        {
        }

        public GeneratedSource(SyntaxNode source, String fileName, Boolean disableWarnings = true)
        {
            var sourceText = source.NormalizeWhitespace()
                .ToFullString();

            var builder = new StringBuilder("// <auto-generated/>\r\n// ")
                .Append(DateTimeOffset.Now);
            if(disableWarnings)
            {
                _ = builder.Append("\r\n#pragma warning disable\r\n");
            }

            _ = builder.Append(sourceText);
            if(disableWarnings)
            {
                _ = builder.Append("\r\n#pragma warning restore\r\n");
            }

            Text = builder.ToString();

            HintName = $"{fileName.Replace('.', '_')}.g.cs";
        }

        public override Boolean Equals(Object obj) => obj is GeneratedSource source && Equals(source);

        public Boolean Equals(GeneratedSource other)
        {
            return Text == other.Text &&
                   HintName == other.HintName;
        }

        public override Int32 GetHashCode()
        {
            var hashCode = 854157587;
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(HintName);
            return hashCode;
        }

        public static Boolean operator ==(GeneratedSource left, GeneratedSource right) => left.Equals(right);

        public static Boolean operator !=(GeneratedSource left, GeneratedSource right) => !(left == right);

        public override String ToString() => Text;
    }
}
