﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct GeneratedType : IEquatable<GeneratedType>
	{
		public GeneratedType(TypeIdentifier identifier, String source) : this()
		{
			Identifier = identifier;
			Source = new GeneratedSource(source, identifier.Name.ToString());
		}
		public GeneratedType(TypeIdentifier identifier, GeneratedSource source) : this()
		{
			Identifier = identifier;
			Source = source;
		}

		public readonly GeneratedSource Source;
		public readonly TypeIdentifier Identifier;

		public override String ToString()
		{
			return $"Generated Type: {Identifier}";
		}

		public override Boolean Equals(Object obj)
		{
			return obj is GeneratedType type && Equals(type);
		}

		public Boolean Equals(GeneratedType other)
		{
			return Source.Equals(other.Source) &&
				   Identifier.Equals(other.Identifier);
		}

		public override Int32 GetHashCode()
		{
			var hashCode = 966028054;
			hashCode = hashCode * -1521134295 + Source.GetHashCode();
			hashCode = hashCode * -1521134295 + Identifier.GetHashCode();
			return hashCode;
		}

		public static Boolean operator ==(GeneratedType left, GeneratedType right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(GeneratedType left, GeneratedType right)
		{
			return !(left == right);
		}
	}
}
