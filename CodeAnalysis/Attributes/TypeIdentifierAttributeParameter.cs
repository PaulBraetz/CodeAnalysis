using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal sealed class TypeIdentifierAttributeParameter : AttributeParameter<TypeIdentifier>
	{
		public TypeIdentifierAttributeParameter(params AttributeParameterDefinition[] definitions) : base(definitions)
		{
		}

		protected override Boolean TryParse(AttributeArgumentSyntax argument, Compilation compilation, out TypeIdentifier value)
		{
			if (!base.TryParse(argument, compilation, out value))
			{
				if (argument.Expression is TypeOfExpressionSyntax typeOfExpression)
				{
					value = CompilationAnalysis.GetTypeIdentifier(typeOfExpression.Type, compilation);
					return true;
				}
			}

			return false;
		}
	}
}
