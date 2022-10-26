using System;

namespace RhoMicro.CodeAnalysis
{
	internal interface IHasTypeParameter
	{
		void SetTypeParameter(String typeName, String parameterName);
		String GetTypeParameter(String parameterName);
	}
}
