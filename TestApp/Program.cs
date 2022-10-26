using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace TestApp
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class TestAttribute : Attribute, IHasTypeProperty, IHasTypeParameter
	{
		public TestAttribute()
		{
		}
		public TestAttribute(char[] arrayParameter, object objectParameter, Type typeParameter, string stringParameter = "Default Value")
		{
			ObjectProperty = objectParameter;
			ArrayProperty = arrayParameter;
			StringProperty = stringParameter;
			TypeProperty = typeParameter;
		}
		public TestAttribute(object objectParameter, char[] arrayParameter, string stringParameter)
		{
			ObjectProperty = objectParameter;
			ArrayProperty = arrayParameter;
			StringProperty = stringParameter;
		}

		public object ObjectProperty { get; set; } = "DefaultObjectValue";
		public char[] ArrayProperty { get; set; } = new char[] { 'a', 'b', 'c' };
		public string StringProperty { get; set; } = "DefaultPropertyValue";
		public Type TypeProperty { get; set; } = typeof(string);

		private readonly IDictionary<string, string> _propertyParameterMap = new Dictionary<string, string>()
		{
			{nameof(TypeProperty), "typeParameter" }
		};
		private readonly IDictionary<string, object> _typeProperties = new Dictionary<string, object>();
		public void SetTypeProperty(string propertyName, object type)
		{
			var parameterName = _propertyParameterMap[propertyName];
			if (_typeProperties.ContainsKey(parameterName))
			{
				_typeProperties[parameterName] = type;
			}
			else
			{
				_typeProperties.Add(parameterName, type);
			}
		}
		public object GetTypeProperty(string propertyName)
		{
			var parameterName = _propertyParameterMap[propertyName];
			return _typeProperties.TryGetValue(parameterName, out var value) ? value : null;
		}

		public void SetTypeParameter(String parameterName, Object type)
		{
			_typeProperties.Add(parameterName, type);
		}

		public Object GetTypeParameter(String parameterName)
		{
			return _typeProperties.TryGetValue(parameterName, out var value) ? value : null;
		}
	}

	[Test(objectParameter: 99, typeParameter: typeof(int), arrayParameter: new char[] { 'd', 'e', 'f', 'g' }, StringProperty = "Property Assigned String Value 2", TypeProperty = typeof(decimal))]
	internal class TestClass
	{
		private const string Prefix = "Prefixed";
		private const string ConstantField = Prefix + nameof(ConstantField);
		public void TestMethod()
		{

		}
	}

	internal class Program
	{
		private const string SOURCE =
@"namespace TestApp
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class TestAttribute : Attribute
	{
		public TestAttribute()
		{
		}
		public TestAttribute(char[] arrayParameter, object objectParameter, Type typeParameter, string stringParameter = ""Default Value"")
		{
			ObjectProperty = objectParameter;
			ArrayProperty = arrayParameter;
			StringProperty = stringParameter;
			TypeProperty = typeParameter;
		}
		public TestAttribute(object objectParameter, char[] arrayParameter, string stringParameter)
		{
			ObjectProperty = objectParameter;
			ArrayProperty = arrayParameter;
			StringProperty = stringParameter;
		}

		public object ObjectProperty { get; set; } = ""DefaultObjectValue"";
		public char[] ArrayProperty { get; set; } = new char[] { 'a', 'b', 'c' };
		public string StringProperty { get; set; } = ""DefaultPropertyValue"";
		public Type TypeProperty { get; set; } = typeof(string);

		private readonly IDictionary<string, object> _typeProperties = new Dictionary<string, object>();
		public void SetTypeProperty(string propertyName, object type)
		{
			_typeProperties.Add(propertyName, type);
		}
		public object GetTypeProperty(string propertyName)
		{
			return _typeProperties[propertyName];
		}


		private readonly IDictionary<string, object> _typeParameters = new Dictionary<string, object>();
		public void SetTypeParameter(String parameterName, Object type)
		{
			_typeParameters.Add(parameterName, type);
		}

		public Object GetTypeParameter(String parameterName)
		{
			return _typeParameters[parameterName];
		}
	}
}";

		private const String TestClass_SOURCE =
@"
using TestApp;

namespace TestNamespace
{
	[Test(objectParameter: 99, arrayParameter: new char[] { 'd', 'e', 'f', 'g' }, stringParameter : ""Property Assigned String Value 2"", typeParameter: typeof(int), TypeProperty=null)]
	internal class TestClass
	{
		private const string Prefix = ""Prefixed"";
		private const string ConstantField = Prefix + nameof(ConstantField);
		public void TestMethod()
		{

		}
	}
}";

		private static AttributeAnalysisUnit<TestAttribute> AnalysisUnit { get; } = new AttributeAnalysisUnit<TestAttribute>(SOURCE);

		static void Main(string[] args)
		{
			var compilation = CSharpCompilation.Create("TestAssembly")
				.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
				.AddSyntaxTrees(
					CSharpSyntaxTree.ParseText(TestClass_SOURCE),
					CSharpSyntaxTree.ParseText(AnalysisUnit.GeneratedType.Source.Text));

			var type = compilation.SyntaxTrees.Select(t => t.GetRoot())
				.SelectMany(t => t.DescendantNodes(n => !(n is BaseTypeDeclarationSyntax)))
				.OfType<BaseTypeDeclarationSyntax>()
				.ToArray()[0];

			var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);

			var attribute = type.AttributeLists.SelectMany(al => al.Attributes).OfAttributeClasses(semanticModel, AnalysisUnit.GeneratedType.Identifier).Single();

			AnalysisUnit.Factory.TryBuild(attribute, semanticModel, out TestAttribute attributeInstance);

			Console.WriteLine(attributeInstance?.GetTypeProperty(nameof(TestAttribute.TypeProperty))??"null");
			Console.WriteLine((Object)attributeInstance?.TypeProperty ?? "null");
		}
	}
}