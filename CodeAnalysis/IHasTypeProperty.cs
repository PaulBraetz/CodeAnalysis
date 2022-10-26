using System;
using System.Collections.Generic;
using System.Text;

namespace RhoMicro.CodeAnalysis
{
	internal interface IHasTypeProperty
	{
		void SetTypeProperty(String propertyName, Object type);
		Object GetTypeProperty(String propertyName);
	}
}
