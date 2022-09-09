using System;

namespace RhoMicro.CodeAnalysis.Attributes
{
	internal abstract class AttributeArgumentBase
	{
		public AttributeArgumentBase(AttributeParameterDefinition matchedDefinition, Boolean isEmpty) : this(isEmpty)
		{
			MatchedDefinition = matchedDefinition;
		}
		private AttributeArgumentBase(Boolean isEmpty)
		{
			IsEmpty = isEmpty;
		}

		public AttributeParameterDefinition MatchedDefinition { get; }
		public Boolean IsEmpty { get; }
		public Boolean IsValid => MatchedDefinition.IsOptional || !IsEmpty;
	}
}
