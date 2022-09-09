using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.CodeAnalysis.Attributes
{
	public abstract class AttributeArgumentBase
	{
		public AttributeArgumentBase(AttributeParameterDefinition matchedDefinition, Boolean isEmpty) : this(isEmpty)
		{
			MatchedDefinition = matchedDefinition;
		}
		private AttributeArgumentBase(Boolean isEmpty)
		{
			IsEmpty = isEmpty;
		}

		public AttributeParameterDefinition MatchedDefinition { get; }
		public Boolean IsEmpty { get; }
		public Boolean IsValid => MatchedDefinition.IsOptional || !IsEmpty;
	}
	public sealed class AttributeArgument<T> : AttributeArgumentBase, IEquatable<AttributeArgument<T>>
	{
		public AttributeArgument(AttributeParameterDefinition matchedDefinition, T value) : base(matchedDefinition, false)
		{
			Value = value;
		}
		public AttributeArgument(AttributeParameterDefinition matchedDefinition) : base(matchedDefinition, true)
		{
			Value = default;
		}

		public T Value { get; }

		public override String ToString()
		{
			StringBuilder valueString = new StringBuilder(Value == null ? "null" : Value is String ? $"\"{Value}\"" : Value.ToString());
			if (!(Value is String) && Value is IEnumerable enumerable)
			{
				valueString.Clear().Append("{");
				foreach (var enumerableItem in enumerable)
				{
					valueString.Append(enumerableItem?.ToString()).Append(", ");
				}
				valueString.Remove(valueString.Length - 2, 2).Append("}");
			}
			return IsValid ? $"{{T: {typeof(T)}, Value: {(IsEmpty ? "Empty" : $"{valueString}}}")}" : "Invalid";
		}

		public override Boolean Equals(Object obj)
		{
			return Equals(obj as AttributeArgument<T>);
		}

		public Boolean Equals(AttributeArgument<T> other)
		{
			return other != null &&
				   MatchedDefinition.Equals(other.MatchedDefinition) &&
				   EqualityComparer<T>.Default.Equals(Value, other.Value);
		}

		public override Int32 GetHashCode()
		{
			var hashCode = -1190155804;
			hashCode = hashCode * -1521134295 + MatchedDefinition.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Value);
			return hashCode;
		}
	}
}
