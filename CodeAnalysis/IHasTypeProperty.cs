/* Unmerged change from project 'AutoForm.Blazor.Analysis'
Before:
using System;
using System.Collections.Generic;
After:
using System.Collections.Generic;
*/

namespace RhoMicro.CodeAnalysis
{
	internal interface IHasTypeProperty
	{
		void SetTypeProperty(System.String propertyName, System.Object type);
		System.Object GetTypeProperty(System.String propertyName);
	}
}
