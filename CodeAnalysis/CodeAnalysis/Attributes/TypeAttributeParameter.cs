//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System;
//using System.Collections.Immutable;
//using System.Linq;

//namespace RhoMicro.CodeAnalysis.Attributes
//{
//	public sealed class TypeAttributeParameter : AttributeParameter<TypeIdentifier>
//	{
//		public TypeAttributeParameter(params AttributeParameterDefinition[] definitions) : base(definitions)
//		{

//		}

//		protected override Boolean TryParse(AttributeArgumentSyntax argument, CompilationAnalyzer analyzer, out TypeIdentifier value)
//		{
//			value = default;
//			var argumentChildren = ImmutableArray<SyntaxNode>.Empty.AddRange(argument.ChildNodes());

//			if (argumentChildren.Length > 0)
//			{
//				if (argumentChildren[0] is TypeOfExpressionSyntax typeofExpression)
//				{
//					value = analyzer.GetTypeIdentifier(typeofExpression.Type);
//				}
//			}

//			return false;
//		}
//	}
//}
