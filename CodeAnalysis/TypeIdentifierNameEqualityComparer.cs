using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal sealed class TypeIdentifierNameEqualityComparer : IEqualityComparer<ITypeIdentifierName>
	{
		public static readonly TypeIdentifierNameEqualityComparer Instance = new TypeIdentifierNameEqualityComparer();
		public System.Boolean Equals(ITypeIdentifierName x, ITypeIdentifierName y)
		{
			System.Boolean result = x == y ||
						 x != null &&
						 y != null &&
						 ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.Equals(x.Parts, y.Parts);

			return result;
		}

		public System.Int32 GetHashCode(ITypeIdentifierName obj)
		{
			System.Int32 hash = ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.GetHashCode(obj.Parts);

			return hash;
		}
	}
}
