using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct Namespace : INamespace, IEquatable<INamespace>
	{
		private Namespace(ImmutableArray<IIdentifierPart> parts)
		{
			Parts = parts;
		}

		public ImmutableArray<IIdentifierPart> Parts { get; }

		public static Namespace Create<T>()
		{
			return Create(typeof(T));
		}
		public static Namespace Create(Type type)
		{
			String[] namespaceParts = type.Namespace.Split('.');
			return Create().AppendRange(namespaceParts);
		}
		public static Namespace Create(ISymbol symbol)
		{
			Namespace result = Create();

			while (symbol != null && symbol.Name != String.Empty)
			{
				if (symbol is INamespaceSymbol)
				{
					result = result.Prepend(symbol.Name);
				}

				symbol = symbol.ContainingNamespace;
			}

			return result;
		}
		public static Namespace Create()
		{
			return new Namespace(ImmutableArray.Create<IIdentifierPart>());
		}

		public Namespace Append(String name)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return this;
			}

			ImmutableArray<IIdentifierPart> parts = GetNextParts().Add(IdentifierPart.Name(name));

			return new Namespace(parts);
		}
		public Namespace Prepend(String name)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return this;
			}

			ImmutableArray<IIdentifierPart> parts = GetPreviousParts().Insert(0, IdentifierPart.Name(name));

			return new Namespace(parts);
		}
		public Namespace PrependRange(IEnumerable<String> names)
		{
			Namespace @namespace = this;
			foreach (String name in names)
			{
				@namespace = @namespace.Prepend(name);
			}

			return @namespace;
		}
		public Namespace AppendRange(IEnumerable<String> names)
		{
			Namespace @namespace = this;
			foreach (String name in names)
			{
				@namespace = @namespace.Append(name);
			}

			return @namespace;
		}

		private ImmutableArray<IIdentifierPart> GetNextParts()
		{
			IdentifierParts.Kind lastKind = Parts.LastOrDefault()?.Kind ?? IdentifierParts.Kind.None;

			Boolean prependSeparator = lastKind == IdentifierParts.Kind.Name;

			return prependSeparator ?
				Parts.Add(IdentifierPart.Period()) :
				Parts;
		}
		private ImmutableArray<IIdentifierPart> GetPreviousParts()
		{
			IdentifierParts.Kind firstKind = Parts.FirstOrDefault()?.Kind ?? IdentifierParts.Kind.None;

			Boolean appendSeparator = firstKind == IdentifierParts.Kind.Name;

			return appendSeparator ?
				Parts.Insert(0, IdentifierPart.Period()) :
				Parts;
		}

		public override String ToString()
		{
			return String.Concat(Parts);
		}

		public override Boolean Equals(Object obj)
		{
			return obj is INamespace @namespace && Equals(@namespace);
		}

		public Boolean Equals(INamespace other)
		{
			return NamespaceEqualityComparer.Instance.Equals(this, other);
		}

		public override Int32 GetHashCode()
		{
			return NamespaceEqualityComparer.Instance.GetHashCode(this);
		}

		public static Boolean operator ==(Namespace left, Namespace right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(Namespace left, Namespace right)
		{
			return !(left == right);
		}

		public static implicit operator String(Namespace @namespace)
		{
			return @namespace.ToString();
		}
	}
}
