using System;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.CodeAnalysis
{
	internal interface IHasTypeProperty
	{
		void SetTypeProperty(String typeName, String propertyName);
		String GetTypeProperty(String propertyName);
	}
}
