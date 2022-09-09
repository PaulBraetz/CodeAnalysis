﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal sealed class AttributeDefinition
	{
		public AttributeDefinition(TypeIdentifier identifier, params AttributeParameterBase[] parameters)
		{
			Identifier = identifier;
			_parameters = ImmutableArray<AttributeParameterBase>.Empty.AddRange(parameters);
		}

		public TypeIdentifier Identifier { get; }

		private readonly ImmutableArray<AttributeParameterBase> _parameters;

		public IEnumerable<AttributeAnnotation> Parse(SyntaxList<AttributeListSyntax> attributes, Compilation compilation)
		{
			var declarations = attributes
				.SelectMany(sl => sl.Attributes)
				.Select(a => new AttributeAnnotation(a, _parameters, compilation));

			return declarations;
		}
	}
}