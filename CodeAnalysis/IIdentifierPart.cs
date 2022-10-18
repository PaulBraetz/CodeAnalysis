using System;

namespace RhoMicro.CodeAnalysis
{
	internal interface IIdentifierPart
	{
		IdentifierParts.Kind Kind { get; }
		String Value { get; }
	}
}