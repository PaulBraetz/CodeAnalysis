using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct TypeIdentifier : ITypeIdentifier, IEquatable<ITypeIdentifier>
	{
		public ITypeIdentifierName Name { get; }
		public INamespace Namespace { get; }

		private TypeIdentifier(ITypeIdentifierName name, INamespace @namespace)
		{
			Name = name;
			Namespace = @namespace;
		}

		public static TypeIdentifier Create<T>()
		{
			return Create(typeof(T));
		}
		public static TypeIdentifier Create(Type type)
		{
			var name = TypeIdentifierName.Create();
			INamespace @namespace = null;

			if (type.IsNested)
			{
				Type parentType = type.DeclaringType;
				TypeIdentifier parentTypeIdentifier = Create(parentType);
				name = name.AppendTypePart(parentTypeIdentifier.Name);
				@namespace = parentTypeIdentifier.Namespace;
			}

			String typeName = type.Name;
			if (type.IsGenericType)
			{
				Int32 iBacktick = typeName.IndexOf('`');
				if (iBacktick > 0)
				{
					typeName = typeName.Remove(iBacktick);
				}
			}

			name = name.AppendNamePart(typeName);

			if (type.IsConstructedGenericType)
			{
				ITypeIdentifier[] genericArguments = type.GenericTypeArguments.Select(Create).OfType<ITypeIdentifier>().ToArray();
				name = name.AppendGenericPart(genericArguments);
			}

			if (type.IsArray)
			{
				name = name.AppendArrayPart();
			}

			if (@namespace == null)
			{
				@namespace = CodeAnalysis.Namespace.Create(type);
			}

			return Create(name, @namespace);
		}
		public static TypeIdentifier Create(TypeSyntax typeSyntax, SemanticModel semanticModel)
		{
			ITypeSymbol symbol = semanticModel.GetDeclaredSymbol(typeSyntax) as ITypeSymbol ??
						 semanticModel.GetTypeInfo(typeSyntax).Type;

			var identifier = TypeIdentifier.Create(symbol);

			return identifier;
		}
		public static TypeIdentifier Create(ITypeSymbol symbol)
		{
			TypeIdentifierName identifier = symbol is ITypeParameterSymbol parameter ?
				 TypeIdentifierName.Create().AppendNamePart(parameter.Name) :
				 TypeIdentifierName.Create(symbol);
			Namespace @namespace = symbol is ITypeParameterSymbol ?
				CodeAnalysis.Namespace.Create() :
				CodeAnalysis.Namespace.Create(symbol);

			return Create(identifier, @namespace);
		}
		public static TypeIdentifier Create(ITypeIdentifierName name, INamespace @namespace)
		{
			return new TypeIdentifier(name, @namespace);
		}

		public override Boolean Equals(Object obj)
		{
			return obj is ITypeIdentifier identifier && Equals(identifier);
		}

		public Boolean Equals(ITypeIdentifier other)
		{
			return TypeIdentifierEqualityComparer.Instance.Equals(this, other);
		}

		public override Int32 GetHashCode()
		{
			return TypeIdentifierEqualityComparer.Instance.GetHashCode(this);
		}

		public override String ToString()
		{
			String namespaceString = Namespace.ToString();
			String nameString = Name.ToString();
			return String.IsNullOrEmpty(namespaceString) ? String.IsNullOrEmpty(nameString) ? null : nameString.ToString() : $"{namespaceString}.{nameString}";
		}

		public static Boolean operator ==(TypeIdentifier left, TypeIdentifier right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(TypeIdentifier left, TypeIdentifier right)
		{
			return !(left == right);
		}

		public static implicit operator String(TypeIdentifier identifier)
		{
			return identifier.ToString();
		}
	}
}
