using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal class ArrayAttributeParameter<T> : AttributeParameter<T[]>
	{
		public ArrayAttributeParameter(params AttributeParameterDefinition[] definitions) : base(definitions)
		{
		}

		protected override Boolean TryParse(AttributeArgumentSyntax argument, Compilation compilation, out T[] value)
		{
			if (!base.TryParse(argument, compilation, out value))
			{
				if (argument.Expression is ArrayCreationExpressionSyntax arrayCreationExpression)
				{
					var itemCount = arrayCreationExpression.Initializer.Expressions.Count;
					var result = new T[itemCount];
					for (var i = 0; i < itemCount; i++)
					{
						var item = arrayCreationExpression.Initializer.Expressions[i];
						if (TryGetConstantValue<T>(item, compilation, out var castElement))
						{
							result[i] = castElement;
						}
						else
						{
							return false;
						}
					}

					value = result;
					return true;
				}
			}

			return false;
		}
	}
}
