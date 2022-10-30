using System;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal sealed class IdentifierPartEqualityComparer : IEqualityComparer<IIdentifierPart>
	{
		public static readonly IdentifierPartEqualityComparer Instance = new IdentifierPartEqualityComparer();
		public Boolean Equals(IIdentifierPart x, IIdentifierPart y)
		{
			var result = x != null &&
						 y != null &&
						(x == y ||
						 x.Value == y.Value);

			return result;
		}

		public Int32 GetHashCode(IIdentifierPart obj)
		{
			var hashcode = obj == null
				? 0
				: -1937169414 + EqualityComparer<String>.Default.GetHashCode(obj.Value);

			return hashcode;
		}
	}
}
