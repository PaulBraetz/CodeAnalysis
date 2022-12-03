using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal sealed class NamespaceEqualityComparer : IEqualityComparer<INamespace>
	{
		public static readonly NamespaceEqualityComparer Instance = new NamespaceEqualityComparer();
		public System.Boolean Equals(INamespace x, INamespace y)
		{
			System.Boolean result = x == y ||
						 x != null &&
						 y != null &&
						 ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.Equals(x.Parts, y.Parts);

			return result;
		}

		public System.Int32 GetHashCode(INamespace obj)
		{
			System.Int32 hash = ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.GetHashCode(obj.Parts);

			return hash;
		}
	}
}
