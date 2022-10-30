using System;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal sealed class NamespaceEqualityComparer : IEqualityComparer<INamespace>
	{
		public static readonly NamespaceEqualityComparer Instance = new NamespaceEqualityComparer();
		public Boolean Equals(INamespace x, INamespace y)
		{
			var result = x != null &&
						 y != null &&
						(x == y ||
						 ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.Equals(x.Parts, y.Parts));

			return result;
		}

		public Int32 GetHashCode(INamespace obj)
		{
			var hash = ImmutableArrayEqualityComparer<IIdentifierPart>.Instance.GetHashCode(obj.Parts);

			return hash;
		}
	}
}
