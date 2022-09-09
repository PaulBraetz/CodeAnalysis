using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal sealed class AttributeAnnotation
	{
		public AttributeAnnotation(AttributeSyntax a, ImmutableArray<AttributeParameterBase> parameters, Compilation compilation)
		{
			_attributeSyntax = a;
			_compilation = compilation;
			_arguments = ImmutableArray<AttributeArgumentBase>.Empty.AddRange(parameters.Select(p => p.ParseWeak(a, compilation)));
		}

		private readonly AttributeSyntax _attributeSyntax;
		private readonly ImmutableArray<AttributeArgumentBase> _arguments;
		private readonly Compilation _compilation;

		public T GetValue<T>(AttributeParameterDefinition definition)
		{
			var argument = GetArgument<T>(definition);
			if (argument.IsValid)
			{
				var value = argument.Value;

				return value;
			}

			throw new InvalidOperationException($"Unable to parse value from: {_attributeSyntax}");
		}
		public AttributeArgument<T> GetArgument<T>(AttributeParameterDefinition definition)
		{
			var argument = _arguments.OfType<AttributeArgument<T>>().FirstOrDefault() ?? new AttributeArgument<T>(definition);

			return argument;
		}
	}
}
