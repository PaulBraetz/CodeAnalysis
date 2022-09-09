//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Immutable;
//using System.Linq;

//namespace RhoMicro.CodeAnalysis.Attributes
//{
//	public sealed class Int32AttributeParameter : AttributeParameter<Int32>
//	{
//		public Int32AttributeParameter(params AttributeParameterDefinition[] definitions) : base(definitions)
//		{

//		}

//		protected override Boolean TryParse(AttributeArgumentSyntax argument, CompilationAnalyzer analyzer, out Int32 value)
//		{
//			value = default;
//			var argumentChildren = ImmutableArray<SyntaxNode>.Empty.AddRange(argument.ChildNodes());

//			if (argumentChildren.Length > 0)
//			{
//				if (argumentChildren[0] is LiteralExpressionSyntax literalExpression &&
//					literalExpression.IsKind(SyntaxKind.NumericLiteralExpression))
//				{
//					return Int32.TryParse(literalExpression.Token.ValueText, out value);
//				}
//			}

//			return false;
//		}
//	}
//}
