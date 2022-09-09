using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis.Attributes
{
	public abstract class AttributeParameterBase
	{
		public ImmutableArray<AttributeParameterDefinition> Definitions { get; }

		protected AttributeParameterBase(ImmutableArray<AttributeParameterDefinition> definitions)
		{
			Definitions = definitions;
		}

		public abstract AttributeArgumentBase ParseWeak(AttributeSyntax attributeSyntax, Compilation compilation);
	}
	public class AttributeParameter<T> : AttributeParameterBase
	{
		public AttributeParameter(params AttributeParameterDefinition[] definitions) : base(ImmutableArray<AttributeParameterDefinition>.Empty.AddRange(definitions))
		{
		}

		public override AttributeArgumentBase ParseWeak(AttributeSyntax attributeSyntax, Compilation compilation)
		{
			return Parse(attributeSyntax, compilation);
		}
		public AttributeArgument<T> Parse(AttributeSyntax attributeSyntax, Compilation compilation)
		{
			var arguments = attributeSyntax?.ArgumentList?.Arguments.ToArray() ?? Array.Empty<AttributeArgumentSyntax>();

			AttributeParameterDefinition definition = default;

			for (var i = 0; i < Definitions.Length; i++)
			{
				definition = Definitions[i];

				for (var j = 0; j < arguments.Length; j++)
				{
					var argument = arguments[j];

					var argumentChildren = ImmutableArray<SyntaxNode>.Empty.AddRange(argument.ChildNodes());
					var firstChild = argumentChildren.FirstOrDefault();

					var match = firstChild is NameEqualsSyntax nameEqualsSyntax ? nameEqualsSyntax.Name.ToString() == definition.Property :
								firstChild is NameColonSyntax nameColonSyntax ? nameColonSyntax.Name.ToString() == definition.Parameter :
								j == definition.Position;

					if (match && TryParse(argument, compilation, out var value))
					{
						return new AttributeArgument<T>(definition, value);
					}
				}
			}

			return new AttributeArgument<T>(definition);
		}
		protected virtual Boolean TryParse(AttributeArgumentSyntax argument, Compilation compilation, out T value)
		{
			return TryGetConstantValue<T>(argument.Expression, compilation, out value);
		}
		protected Boolean TryGetConstantValue<TExpression>(ExpressionSyntax expression, Compilation compilation, out TExpression value)
		{
			var semanticModel = compilation.GetSemanticModel(expression.SyntaxTree);
			var constantValue = semanticModel.GetConstantValue(expression);

			value = default;

			if (constantValue.HasValue)
			{
				if (constantValue.Value == null)
				{
					return true;
				}
				else if (constantValue.Value is TExpression castValue)
				{
					value = castValue;
					return true;
				}
			}

			return false;
		}
	}
}
