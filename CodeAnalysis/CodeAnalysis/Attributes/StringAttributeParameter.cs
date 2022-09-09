//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Xml.Linq;

//namespace RhoMicro.CodeAnalysis.Attributes
//{
//	public sealed class StringAttributeParameter : AttributeParameter<String>
//	{
//		public StringAttributeParameter(params AttributeParameterDefinition[] definitions) : base(definitions)
//		{

//		}

//		protected override Boolean TryParse(AttributeArgumentSyntax argument, CompilationAnalyzer analyzer, out String value)
//		{
//			value = default;
//			var result = false;
//			var argumentChildren = ImmutableArray<SyntaxNode>.Empty.AddRange(argument.ChildNodes());

//			if (argumentChildren.Length > 0)
//			{
//				var constantValue = analyzer.Compilation.GetSemanticModel(argument.SyntaxTree).GetConstantValue(argument.Expression);

//				var node = argumentChildren.Length == 1 ? argumentChildren[0] : argumentChildren[1];

//				if (node is LiteralExpressionSyntax literalExpression &&
//					 literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
//				{
//					value = literalExpression.Token.ValueText;
//					result = true;
//				}
//				else if (node is InvocationExpressionSyntax invocation &&
//					invocation.Expression is IdentifierNameSyntax identifierName &&
//					identifierName.Identifier.ValueText == "nameof")
//				{
//					value = invocation.ArgumentList.Arguments.SingleOrDefault()?.GetText().ToString();
//					result = true;
//				}
//				else
//				{
//					if (constantValue.HasValue)
//					{
//						value = (String)constantValue.Value;
//						result = true;
//					}
//				}
//			}

//			return result;
//		}
//	}
//}
