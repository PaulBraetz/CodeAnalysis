﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct GeneratedSource : IEquatable<GeneratedSource>
	{
		public readonly String Text;
		public readonly String HintName;

		public GeneratedSource(String source, String fileName)
		{
			source =
	$@"// <auto-generated/>
// {DateTimeOffset.Now}
{source}";

			Text = CSharpSyntaxTree.ParseText(source)
				.GetRoot()
				.NormalizeWhitespace()
				.ToFullString();

			HintName = $"{fileName}.g.cs";
		}

		public override Boolean Equals(Object obj)
		{
			return obj is GeneratedSource source && Equals(source);
		}

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

		public static Boolean operator ==(GeneratedSource left, GeneratedSource right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(GeneratedSource left, GeneratedSource right)
		{
			return !(left == right);
		}

		public override String ToString()
		{
			return Text;
		}
	}
}
