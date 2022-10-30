using System;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct IdentifierPart : IIdentifierPart, IEquatable<IIdentifierPart>
	{
		public IdentifierParts.Kind Kind { get; }
		public String Value { get; }

		private IdentifierPart(String name, IdentifierParts.Kind kind)
		{
			Kind = kind;

			switch (Kind)
			{
				case IdentifierParts.Kind.Array:
					Value = "[]";
					break;
				case IdentifierParts.Kind.GenericOpen:
					Value = "<";
					break;
				case IdentifierParts.Kind.GenericClose:
					Value = ">";
					break;
				case IdentifierParts.Kind.Period:
					Value = ".";
					break;
				case IdentifierParts.Kind.Comma:
					Value = ", ";
					break;
				default:
					Value = name;
					break;
			}
		}
		private IdentifierPart(IdentifierParts.Kind kind) : this(null, kind) { }

		public static IdentifierPart Name(String name)
		{
			return new IdentifierPart(name, IdentifierParts.Kind.Name);
		}
		public static IdentifierPart Array()
		{
			return new IdentifierPart(IdentifierParts.Kind.Array);
		}
		public static IdentifierPart GenericOpen()
		{
			return new IdentifierPart(IdentifierParts.Kind.GenericOpen);
		}
		public static IdentifierPart GenericClose()
		{
			return new IdentifierPart(IdentifierParts.Kind.GenericClose);
		}
		public static IdentifierPart Period()
		{
			return new IdentifierPart(IdentifierParts.Kind.Period);
		}
		public static IdentifierPart Comma()
		{
			return new IdentifierPart(IdentifierParts.Kind.Comma);
		}
		public override String ToString()
		{
			return Value ?? String.Empty;
		}

		public override Boolean Equals(Object obj)
		{
			return obj is IIdentifierPart identifierPart && Equals(identifierPart);
		}

		public Boolean Equals(IIdentifierPart other)
		{
			return IdentifierPartEqualityComparer.Instance.Equals(this, other);
		}

		public override Int32 GetHashCode()
		{
			return IdentifierPartEqualityComparer.Instance.GetHashCode(this);
		}

		public static implicit operator String(IdentifierPart @namespace)
		{
			return @namespace.ToString();
		}

		public static Boolean operator ==(IdentifierPart left, IdentifierPart right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(IdentifierPart left, IdentifierPart right)
		{
			return !(left == right);
		}
	}
}
