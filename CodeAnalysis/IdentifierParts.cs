using System;

namespace RhoMicro.CodeAnalysis
{
	internal static class IdentifierParts
	{
		public enum Kind : Byte
		{
			None,
			Array,
			GenericOpen,
			GenericClose,
			Comma,
			Period,
			Name
		}
	}
}