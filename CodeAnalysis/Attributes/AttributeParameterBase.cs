using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal abstract class AttributeParameterBase
	{
		public ImmutableArray<AttributeParameterDefinition> Definitions { get; }

		protected AttributeParameterBase(ImmutableArray<AttributeParameterDefinition> definitions)
		{
			Definitions = definitions;
		}

		public abstract AttributeArgumentBase ParseWeak(AttributeSyntax attributeSyntax, Compilation compilation);
	}
}
