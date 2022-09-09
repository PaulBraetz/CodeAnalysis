using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhoMicro.CodeAnalysis
{
	public static class CompilationAnalysis
	{
		public static IEnumerable<BaseTypeDeclarationSyntax> GetTypeDeclarations(Compilation compilation, IEnumerable<TypeIdentifier> include = null, IEnumerable<TypeIdentifier> exclude = null)
		{
			var typeDeclarations = compilation.SyntaxTrees.Select(s => s.GetRoot())
				.SelectMany(r => r.DescendantNodes(n => !(n is BaseTypeDeclarationSyntax)))
				.OfType<BaseTypeDeclarationSyntax>();

			if (include == null)
			{
				include = Array.Empty<TypeIdentifier>();
			}

			if (exclude == null)
			{
				exclude = Array.Empty<TypeIdentifier>();
			}

			if (!exclude.Any() && !include.Any())
			{
				return typeDeclarations;
			}

			return typeDeclarations.Where(d => !exclude.Any(a => HasAttribute(d.AttributeLists, d, a)) && include.Any(a => HasAttribute(d.AttributeLists, d, a)));
		}

		public static IEnumerable<FieldDeclarationSyntax> GetFieldDeclarations(BaseTypeDeclarationSyntax typeDeclaration, IEnumerable<TypeIdentifier> include = null, IEnumerable<TypeIdentifier> exclude = null)
		{
			var fields = typeDeclaration.ChildNodes().OfType<FieldDeclarationSyntax>();

			if (include == null)
			{
				include = Array.Empty<TypeIdentifier>();
			}

			if (exclude == null)
			{
				exclude = Array.Empty<TypeIdentifier>();
			}

			if (!exclude.Any() && !include.Any())
			{
				return fields;
			}

			return fields.Where(d => !exclude.Any(a => HasAttribute(d.AttributeLists, d, a)) && include.Any(a => HasAttribute(d.AttributeLists, d, a)));
		}

		public static IEnumerable<PropertyDeclarationSyntax> GetPropertyDeclarations(BaseTypeDeclarationSyntax typeDeclaration, IEnumerable<TypeIdentifier> include = null, IEnumerable<TypeIdentifier> exclude = null)
		{
			var properties = typeDeclaration.ChildNodes().OfType<PropertyDeclarationSyntax>();

			if (include == null)
			{
				include = Array.Empty<TypeIdentifier>();
			}

			if (exclude == null)
			{
				exclude = Array.Empty<TypeIdentifier>();
			}

			if (!exclude.Any() && !include.Any())
			{
				return properties;
			}

			return properties.Where(d => !exclude.Any(a => HasAttribute(d.AttributeLists, d, a)) && include.Any(a => HasAttribute(d.AttributeLists, d, a)));
		}

		public static IEnumerable<AttributeAnnotation> GetAttributes(SyntaxList<AttributeListSyntax> attributeLists, SyntaxNode node, Compilation compilation, IEnumerable<AttributeDefinition> definitions)
		{
			var declarations = definitions
				.Where(d => HasAttribute(attributeLists, node, d.Identifier))
				.SelectMany(d => d.Parse(attributeLists, compilation));

			return declarations;
		}

		public static Boolean HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, SyntaxNode node, TypeIdentifier attributeIdentifier)
		{
			var availableUsings = GetAvailableUsings(node);
			var usingNamespace = availableUsings.Contains(attributeIdentifier.Namespace);

			return attributeLists.SelectMany(al => al.Attributes).Any(equals);

			Boolean equals(AttributeSyntax attributeSyntax)
			{
				return attributeSyntax.Name.ToString() == attributeIdentifier.ToString() ||
					usingNamespace && attributeSyntax.Name.ToString() == attributeIdentifier.Name.ToString();
			}
		}

		public static IEnumerable<Namespace> GetAvailableUsings(SyntaxNode node)
		{
			var result = new List<Namespace>();

			while (node.Parent != null)
			{
				var namespaces = node.Parent.ChildNodes().OfType<UsingDirectiveSyntax>();

				foreach (var @namespace in namespaces)
				{
					var item = Namespace.Create()
						.WithRange(@namespace.Name.ToString().Split('.'));

					result.Add(item);
				}

				node = node.Parent;
			}

			return result;
		}

		public static TypeIdentifier GetTypeIdentifier(TypeSyntax type, Compilation compilation)
		{
			var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);
			var symbol = semanticModel.GetDeclaredSymbol(type) as ITypeSymbol ?? semanticModel.GetTypeInfo(type).Type;

			var identifier = TypeIdentifier.Create(symbol);

			return identifier;
		}
		public static TypeIdentifier GetTypeIdentifier(PropertyDeclarationSyntax property, Compilation compilation)
		{
			var semanticModel = compilation.GetSemanticModel(property.SyntaxTree);
			var symbol = semanticModel.GetDeclaredSymbol(property).Type;

			var identifier = TypeIdentifier.Create(symbol);

			return identifier;
		}
		public static TypeIdentifier GetTypeIdentifier(BaseTypeDeclarationSyntax declaration, Compilation compilation)
		{
			var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
			var symbol = semanticModel.GetDeclaredSymbol(declaration);

			var identifier = TypeIdentifier.Create(symbol);

			return identifier;
		}
	}
}
