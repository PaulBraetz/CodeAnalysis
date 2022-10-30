﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct TypeIdentifierName : ITypeIdentifierName, IEquatable<ITypeIdentifierName>
	{
		public ImmutableArray<IIdentifierPart> Parts { get; }

		private TypeIdentifierName(ImmutableArray<IIdentifierPart> parts)
		{
			Parts = parts;
		}

		public static TypeIdentifierName Create<T>()
		{
			return Create(typeof(T));
		}
		public static TypeIdentifierName Create(Type type)
		{
			return Create().AppendNamePart(type.Name);
		}
		public static TypeIdentifierName Create(ITypeSymbol symbol)
		{
			var result = Create();

			if (symbol.ContainingType != null)
			{
				var containingType = Create(symbol.ContainingType);
				result = result.AppendTypePart(containingType);
			}

			var flag = false;
			if (symbol is IArrayTypeSymbol arraySymbol)
			{
				flag = true;
				symbol = arraySymbol.ElementType;
			}

			result = result.AppendNamePart(symbol.Name);

			if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.TypeArguments.Any())
			{
				var arguments = new ITypeIdentifier[namedSymbol.TypeArguments.Length];

				for (var i = 0; i < arguments.Length; i++)
				{
					var typeArgument = namedSymbol.TypeArguments[i];
					TypeIdentifier argument;
					if (SymbolEqualityComparer.Default.Equals(typeArgument.ContainingType, namedSymbol))
					{
						argument = TypeIdentifier.Create(TypeIdentifierName.Create().AppendNamePart(typeArgument.ToString()), Namespace.Create());
					}
					else
					{
						argument = TypeIdentifier.Create(typeArgument);
					}

					arguments[i] = argument;
				}

				result = result.AppendGenericPart(arguments);
			}

			if (flag)
			{
				result = result.AppendArrayPart();
			}

			return result;
		}
		public static TypeIdentifierName Create()
		{
			return new TypeIdentifierName(ImmutableArray<IIdentifierPart>.Empty);
		}

		public TypeIdentifierName AppendTypePart(ITypeIdentifierName type)
		{
			var parts = GetNextParts(IdentifierParts.Kind.Name)
				.AddRange(type.Parts);

			return new TypeIdentifierName(parts);
		}
		public TypeIdentifierName AppendNamePart(String name)
		{
			var parts = GetNextParts(IdentifierParts.Kind.Name)
				.Add(IdentifierPart.Name(name));

			return new TypeIdentifierName(parts);
		}
		public TypeIdentifierName AppendGenericPart(ITypeIdentifier[] arguments)
		{
			var parts = GetNextParts(IdentifierParts.Kind.GenericOpen)
				.Add(IdentifierPart.GenericOpen());

			var typesArray = arguments ?? Array.Empty<ITypeIdentifier>();

			for (var i = 0; i < typesArray.Length; i++)
			{
				var type = typesArray[i];

				if (type.Namespace.Parts.Any())
				{
					parts = parts.AddRange(type.Namespace.Parts)
								 .Add(IdentifierPart.Period());
				}

				parts = parts.AddRange(type.Name.Parts);

				if (i != typesArray.Length - 1)
				{
					parts = parts.Add(IdentifierPart.Comma());
				}
			}

			parts = parts.Add(IdentifierPart.GenericClose());

			return new TypeIdentifierName(parts);
		}
		public TypeIdentifierName AppendArrayPart()
		{
			var parts = GetNextParts(IdentifierParts.Kind.Array).Add(IdentifierPart.Array());
			return new TypeIdentifierName(parts);
		}

		private ImmutableArray<IIdentifierPart> GetNextParts(IdentifierParts.Kind nextKind)
		{
			var lastKind = Parts.LastOrDefault()?.Kind ?? IdentifierParts.Kind.None;

			var prependSeparator = nextKind == IdentifierParts.Kind.Name &&
									(lastKind == IdentifierParts.Kind.GenericOpen ||
									lastKind == IdentifierParts.Kind.Name);

			if (prependSeparator)
			{
				return Parts.Add(IdentifierPart.Period());
			}

			return Parts;
		}

		public override String ToString()
		{
			return String.Concat(Parts);
		}

		public override Boolean Equals(Object obj)
		{
			return obj is ITypeIdentifierName name && Equals(name);
		}

		public Boolean Equals(ITypeIdentifierName other)
		{
			return TypeIdentifierNameEqualityComparer.Instance.Equals(this, other);
		}

		public override Int32 GetHashCode()
		{
			return TypeIdentifierNameEqualityComparer.Instance.GetHashCode(this);
		}

		public static Boolean operator ==(TypeIdentifierName left, TypeIdentifierName right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(TypeIdentifierName left, TypeIdentifierName right)
		{
			return !(left == right);
		}

		public static implicit operator String(TypeIdentifierName name)
		{
			return name.ToString();
		}
	}
}
