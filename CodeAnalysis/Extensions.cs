using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RhoMicro.CodeAnalysis
{
	internal static class Extensions
	{
		public static String ToNonGenericString(this TypeIdentifier identifier)
		{
			String result = String.Concat(identifier.Namespace.Parts.Append(IdentifierPart.Period()).Concat(identifier.Name.Parts.TakeWhile(p => p.Kind == IdentifierParts.Kind.Name || p.Kind == IdentifierParts.Kind.Period)));

			return result;
		}

		public static IEnumerable<IEnumerable<T>> Subsets<T>(this IEnumerable<T> collection)
		{
			T[] arr = collection.ToArray();
			Double subsetCount = Math.Pow(2, arr.Length);

			for (Int32 i = 0; i < subsetCount; i++)
			{
				yield return collection.Where((element, position) => ((1 << position) & i) == 0);
			}
		}
		public static T[][] ToArrays<T>(this IEnumerable<IEnumerable<T>> collections)
		{
			return collections.Select(s => s.ToArray()).ToArray();
		}

		public static void AddSource(this GeneratorPostInitializationContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Text);
		}
		public static void AddSources(this GeneratorPostInitializationContext context, IEnumerable<GeneratedSource> sources)
		{
			foreach (GeneratedSource source in sources)
			{
				context.AddSource(source);
			}
		}
		public static void AddSources(this GeneratorPostInitializationContext context, params GeneratedSource[] sources)
		{
			foreach (GeneratedSource source in sources)
			{
				context.AddSource(source);
			}
		}

		public static void AddSource(this GeneratorExecutionContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Text);
		}
		public static void AddSources(this GeneratorExecutionContext context, IEnumerable<GeneratedSource> sources)
		{
			foreach (GeneratedSource source in sources)
			{
				context.AddSource(source);
			}
		}
		public static void AddSources(this GeneratorExecutionContext context, params GeneratedSource[] sources)
		{
			foreach (GeneratedSource source in sources)
			{
				context.AddSource(source);
			}
		}

		public static INamedTypeSymbol GetSymbol(this Compilation compilation, TypeIdentifier identifier)
		{
			return compilation.GetTypeByMetadataName(identifier.ToString());
		}

		public static TypeSyntax AsSyntax(this TypeIdentifier typeIdentifier)
		{
			TypeSyntax syntax = SyntaxFactory.ParseTypeName(typeIdentifier);

			return syntax;
		}
		public static TypeSyntax AsSyntax(this Type type)
		{
			TypeSyntax syntax = TypeIdentifier.Create(type).AsSyntax();

			return syntax;
		}

		public static ITypeIdentifier GetIdentifier(this Type type)
		{
			ITypeIdentifier identifier = type != null ?
				(ITypeIdentifier)TypeIdentifier.Create(type) :
				throw new ArgumentNullException(nameof(type));

			return identifier;
		}

		#region AttributeSyntax Operations
		public static Boolean Matches(this AttributeSyntax attribute, SemanticModel semanticModel, ConstructorInfo constructor)
		{
			IEnumerable<AttributeArgumentSyntax> arguments = (IEnumerable<AttributeArgumentSyntax>)attribute.ArgumentList?.Arguments ?? Array.Empty<AttributeArgumentSyntax>();

			Boolean match = matchesType() && matchesParameters() && matchesProperties();

			return match;

			Boolean matchesType()
			{
				Boolean typeMatch = attribute.IsType(semanticModel, TypeIdentifier.Create(constructor.DeclaringType));

				return typeMatch;
			}

			Boolean matchesParameters()
			{
				AttributeArgumentSyntax[] unmatchedArguments = arguments.Where(a => a.NameEquals == null).ToArray();

				Int32 position = 0;
				var positionalParameters = constructor.GetParameters().ToDictionary(p => position++, p => p);
				var namedParameters = constructor.GetParameters().ToDictionary(p => p.Name, p => p);

				for (position = 0; position < unmatchedArguments.Length; position++)
				{
					AttributeArgumentSyntax unmatchedArgument = unmatchedArguments[position];

					if (unmatchedArgument.NameColon == null)
					{
						if (!positionalParameters.TryGetValue(position, out ParameterInfo positionalParameter))
						{
							return false;
						}

						_ = namedParameters.Remove(positionalParameter.Name);
						_ = positionalParameters.Remove(position);
					}
					else
					{
						String argumentName = unmatchedArgument.NameColon.Name.Identifier.ToString();
						if (!namedParameters.TryGetValue(argumentName, out ParameterInfo namedParameter))
						{
							return false;
						}

						_ = namedParameters.Remove(argumentName);
						_ = positionalParameters.Remove(positionalParameters.Single(kvp => kvp.Value.Name == argumentName).Key);
					}

					if (unmatchedArgument.Expression is TypeOfExpressionSyntax &&
						!constructor.DeclaringType.ImplementsMethodsSemantically<IHasTypeParameter>())
					{
						return false;
					}
				}

				Boolean noneLeft = !positionalParameters.Any(kvp => !kvp.Value.IsOptional);

				return noneLeft;
			}

			Boolean matchesProperties()
			{
				var properties = constructor.DeclaringType.GetProperties()
					.Where(p => p.CanWrite)
					.ToDictionary(p => p.Name, p => p);

				Boolean allValid = arguments.Where(a => a.NameEquals != null)
					.All(a =>
						properties.ContainsKey(a.NameEquals.Name.Identifier.ToString()) &&
						(!(a.Expression is TypeOfExpressionSyntax) || constructor.DeclaringType.ImplementsMethodsSemantically<IHasTypeProperty>()));

				return allValid;
			}
		}

		public static Boolean TryGetMethodSemantically(this Type type, MethodInfo referenceMethod, out MethodInfo declaredMethod)
		{
			declaredMethod = type.GetMethod(referenceMethod.Name,
											referenceMethod.GetParameters().Select(p => p.ParameterType).ToArray());

			return declaredMethod != null;
		}

		public static Boolean ImplementsMethodsSemantically<T>(this Type type)
		{
			Boolean match = typeof(T).GetMethods().All(rm => type.TryGetMethodSemantically(rm, out MethodInfo _));

			return match;
		}

		public static IEnumerable<AttributeSyntax> OfAttributeClasses(this IEnumerable<AttributeSyntax> attributes, SemanticModel semanticModel, params TypeIdentifier[] identifiers)
		{
			HashSet<String> requiredTypes = new(identifiers.SelectMany(GetVariations));
			IEnumerable<AttributeSyntax> foundAttributes = attributes.Where(a => requiredTypes.Contains(semanticModel.GetTypeInfo(a).Type?.ToDisplayString()));

			return foundAttributes;
		}
		public static IEnumerable<AttributeSyntax> OfAttributeClasses(this IEnumerable<AttributeListSyntax> attributeLists, SemanticModel semanticModel, params TypeIdentifier[] identifiers)
		{
			HashSet<String> requiredTypes = new(identifiers.SelectMany(GetVariations));
			IEnumerable<AttributeSyntax> foundAttributes = attributeLists.SelectMany(al => al.Attributes).Where(a => requiredTypes.Contains(semanticModel.GetTypeInfo(a).Type?.ToDisplayString()));

			return foundAttributes;
		}
		public static Boolean HasAttributes(this IEnumerable<AttributeListSyntax> attributeLists, SemanticModel semanticModel, params TypeIdentifier[] identifiers)
		{
			Boolean match = attributeLists.OfAttributeClasses(semanticModel, identifiers).Any();

			return match;
		}

		public static Boolean IsType(this AttributeSyntax attribute, SemanticModel semanticModel, TypeIdentifier identifier)
		{
			Boolean match = semanticModel.GetTypeInfo(attribute).Type?.ToDisplayString() == identifier.ToString();

			return match;
		}
		public static Boolean TryParseArgument<T>(this AttributeSyntax attribute, SemanticModel semanticModel, out T value, Int32 position = -1, String propertyName = null, String parameterName = null)
		{
			Optional<Object> arg = attribute.GetArgument(semanticModel, position, propertyName, parameterName);

			Boolean result = TryParse(arg, out value);

			return result;
		}
		public static Boolean TryParseArrayArgument<T>(this AttributeSyntax attribute, SemanticModel semanticModel, out T[] value, Int32 position = -1, String propertyName = null, String parameterName = null)
		{
			Optional<Object> arg = attribute.GetArgument(semanticModel, position, propertyName, parameterName);

			Boolean result = TryParseArray(arg, out value);

			return result;
		}

		public static Boolean TryParse<T>(this Optional<Object> constant, out T value)
		{
			if (constant.HasValue)
			{
				if (constant.Value is T castValue)
				{
					value = castValue;
					return true;
				}

				if (constant.Value == null)
				{
					value = default;
					return true;
				}

				try
				{
					value = (T)constant.Value;
					return true;
				}
				catch { }
			}

			value = default;
			return false;
		}
		public static Boolean TryParseArray<T>(this Optional<Object> constant, out T[] values)
		{
			if (!constant.HasValue)
			{
				values = null;
				return false;
			}

			Object[] elements = constant.Value is Object[] objectArray ? objectArray : constant.Value is T ? new Object[] { constant.Value } : Array.Empty<Object>();
			var tempValues = new T[elements.Length];

			for (Int32 i = 0; i < elements.Length; i++)
			{
				Object element = elements[i];

				if (element is T castValue)
				{
					tempValues[i] = castValue;
				}
				else if (element == null)
				{
					tempValues[i] = default;
				}
				else
				{
					values = null;
					return false;
				}
			}

			values = tempValues;
			return true;
		}

		public static Optional<Object> GetArgument(this AttributeSyntax attribute, SemanticModel semanticModel, Int32 position = -1, String propertyName = null, String parameterName = null)
		{
			IEnumerable<AttributeArgumentSyntax> arguments = (IEnumerable<AttributeArgumentSyntax>)attribute.ArgumentList?.Arguments ?? Array.Empty<AttributeArgumentSyntax>();
			foreach (AttributeArgumentSyntax argument in arguments)
			{
				if (argument.NameEquals != null)
				{
					if (argument.NameEquals.Name.Identifier.ValueText.Equals(propertyName))
					{
						return getConstantValue();
					}
				}
				else if (argument.NameColon != null)
				{
					if (argument.NameColon.Name.Identifier.ValueText.Equals(parameterName))
					{
						return getConstantValue();
					}
				}
				else if (position-- == 0)
				{
					return getConstantValue();
				}

				Optional<Object> getConstantValue()
				{
					Optional<Object> result = semanticModel.GetConstantValue(argument.Expression);

					if (argument.Expression is ArrayCreationExpressionSyntax arrayCreationExpression)
					{
						Object[] elements = arrayCreationExpression.Initializer?
							.Expressions
							.Select(e => semanticModel.GetConstantValue(e))
							.Where(o => o.HasValue)
							.Select(o => o.Value)
							.ToArray() ?? Array.Empty<Object>();

						result = new Optional<Object>(elements);
					}
					else if (argument.Expression is ObjectCreationExpressionSyntax objectCreationExpression)
					{
						result = new Optional<Object>(new Object());
					}
					else if (argument.Expression is TypeOfExpressionSyntax typeOfExpression)
					{
						result = new Optional<Object>(TypeIdentifier.Create(typeOfExpression.Type, semanticModel));
					}

					return result;
				}
			}

			return new Optional<Object>();
		}
		#endregion

		#region AttributeData Operations
		public static IEnumerable<AttributeData> OfAttributeClasses(this IEnumerable<AttributeData> attributes, params TypeIdentifier[] identifiers)
		{
			HashSet<String> requiredTypes = new(identifiers.Select(i => i.ToString()));
			IEnumerable<AttributeData> foundAttributes = attributes.Where(a => requiredTypes.Contains(a.AttributeClass.ToDisplayString()));

			return foundAttributes;
		}
		public static Boolean HasAttributes(this SyntaxNode node, SemanticModel semanticModel, params TypeIdentifier[] identifiers)
		{
			Boolean match = semanticModel.GetDeclaredSymbol(node)?.HasAttributes(identifiers)
				?? throw new ArgumentException($"{nameof(node)} was not declared in {nameof(semanticModel)}.");

			return match;
		}
		public static Boolean HasAttributes(this ISymbol symbol, params TypeIdentifier[] identifiers)
		{
			Boolean match = symbol.GetAttributes().OfAttributeClasses(identifiers).Any();

			return match;
		}

		public static Boolean IsType(this AttributeData attribute, TypeIdentifier identifier)
		{
			Boolean match = attribute.AttributeClass.ToDisplayString() == identifier.ToString();

			return match;
		}
		public static Boolean TryParseArgument<T>(this AttributeData attribute, out T value, Int32 position = -1, String propertyName = null)
		{
			TypedConstant arg = attribute.GetArgument(position, propertyName);

			Boolean result = TryParse(arg, out value);

			return result;
		}
		public static Boolean TryParseArrayArgument<T>(this AttributeData attribute, out T[] value, Int32 position = -1, String propertyName = null)
		{
			TypedConstant arg = attribute.GetArgument(position, propertyName);

			Boolean result = TryParseArray(arg, out value);

			return result;
		}

		public static Boolean TryParse<T>(this TypedConstant constant, out T value)
		{
			if (constant.Kind != TypedConstantKind.Error && constant.Kind != TypedConstantKind.Array)
			{
				if (constant.Value is T castValue)
				{
					value = castValue;
					return true;
				}
			}

			value = default;
			return false;
		}
		public static Boolean TryParseArray<T>(this TypedConstant constant, out T[] values)
		{
			if (constant.Kind is TypedConstantKind.Array)
			{
				T[] parseResults = constant.Values
					.Select(c => (success: TryParse(c, out T value), value))
					.Where(r => r.success)
					.Select(r => r.value)
					.ToArray();

				if (parseResults.Length == constant.Values.Length)
				{
					values = parseResults;
					return true;
				}
			}

			values = default;
			return false;
		}

		public static TypedConstant GetArgument(this AttributeData attribute, Int32 position = -1, String propertyName = null)
		{
			KeyValuePair<String, TypedConstant> namedArgument = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == propertyName);
			if (namedArgument.Value.Kind != TypedConstantKind.Error)
			{
				return namedArgument.Value;
			}

			TypedConstant positionalArgument = attribute.ConstructorArguments.Skip(position).FirstOrDefault();

			return positionalArgument;
		}
		#endregion
		private static IEnumerable<String> GetVariations(TypeIdentifier attributeIdentifier)
		{
			String baseVariation = attributeIdentifier.ToString();
			if (baseVariation.EndsWith("Attribute"))
			{
				return new[] { baseVariation, baseVariation.Substring(0, baseVariation.Length - "Attribute".Length) };
			}

			return new[] { baseVariation };
		}
	}
}
