using System;

namespace RhoMicro.CodeAnalysis
{
	internal interface IHasTypeParameter
	{
		void SetTypeParameter(String parameterName, Object type);
		Object GetTypeParameter(String parameterName);
	}
}
