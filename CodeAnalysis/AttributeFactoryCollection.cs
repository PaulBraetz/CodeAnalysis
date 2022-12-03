using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal sealed class AttributeFactoryCollection<T> : IAttributeFactory<T>
	{
		private readonly List<IAttributeFactory<T>> _factories = new();

		public AttributeFactoryCollection<T> Add(IAttributeFactory<T> factory)
		{
			_factories.Add(factory);
			return this;
		}

		public System.Boolean TryBuild(AttributeSyntax attributeData, SemanticModel semanticModel, out T attribute)
		{
			foreach (IAttributeFactory<T> factory in _factories)
			{
				if (factory.TryBuild(attributeData, semanticModel, out T builtAttribute))
				{
					attribute = builtAttribute;
					return true;
				}
			}

			attribute = default;
			return false;
		}
	}
}
