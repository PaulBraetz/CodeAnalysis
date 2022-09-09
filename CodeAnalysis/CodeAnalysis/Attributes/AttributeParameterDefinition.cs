using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis.Attributes
{
	public readonly struct AttributeParameterDefinition : IEquatable<AttributeParameterDefinition>
	{
		public readonly string Property;
		public readonly string Parameter;
		public readonly int Position;
		public readonly Boolean IsOptional;

		public AttributeParameterDefinition(System.String property = null, System.String parameter = null, System.Int32 position = -1, Boolean isOptional = false)
		{
			Property = property;
			Parameter = parameter;
			Position = position;
			IsOptional = isOptional;
		}

		public override String ToString()
		{
			return $"{{Position: {Position},Parameter: {Parameter}, Property: {Property}, IsOptional: {IsOptional}}}";
		}

		public override Boolean Equals(Object obj)
		{
			return obj is AttributeParameterDefinition definition && Equals(definition);
		}

		public Boolean Equals(AttributeParameterDefinition other)
		{
			return IsOptional == other.IsOptional &&
				   Property != null && Property == other.Property ||
				   Parameter != null && Parameter == other.Parameter ||
				   Position > -1 && Position == other.Position;
		}

		public override Int32 GetHashCode()
		{
			var hashCode = 139411394;
			hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Property);
			hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Parameter);
			hashCode = hashCode * -1521134295 + Position.GetHashCode();
			return hashCode;
		}

		public static Boolean operator ==(AttributeParameterDefinition left, AttributeParameterDefinition right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(AttributeParameterDefinition left, AttributeParameterDefinition right)
		{
			return !(left == right);
		}
	}
}
