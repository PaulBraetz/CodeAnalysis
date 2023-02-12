using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Attributes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestApp
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class TestAttribute : Attribute, IHasTypeProperty, IHasTypeParameter
    {
        public TestAttribute()
        {
        }
        public TestAttribute(Char[] arrayParameter, Object objectParameter, Type typeParameter, String stringParameter = "Default Value")
        {
            ObjectProperty = objectParameter;
            ArrayProperty = arrayParameter;
            StringProperty = stringParameter;
            TypeProperty = typeParameter;
        }
        public TestAttribute(Object objectParameter, Char[] arrayParameter, String stringParameter)
        {
            ObjectProperty = objectParameter;
            ArrayProperty = arrayParameter;
            StringProperty = stringParameter;
        }

        public Object ObjectProperty { get; set; } = "DefaultObjectValue";
        public Char[] ArrayProperty { get; set; } = new Char[] { 'a', 'b', 'c' };
        public String StringProperty { get; set; } = "DefaultPropertyValue";
        public Type TypeProperty { get; set; } = typeof(String);

        private readonly IDictionary<String, String> _propertyParameterMap = new Dictionary<String, String>()
        {
            {nameof(TypeProperty), "typeParameter" }
        };
        private readonly IDictionary<String, Object> _typeProperties = new Dictionary<String, Object>();
        public void SetTypeProperty(String propertyName, Object type)
        {
            var parameterName = _propertyParameterMap[propertyName];
            if(_typeProperties.ContainsKey(parameterName))
            {
                _typeProperties[parameterName] = type;
            } else
            {
                _typeProperties.Add(parameterName, type);
            }
        }
        public Object GetTypeProperty(String propertyName)
        {
            var parameterName = _propertyParameterMap[propertyName];
            return _typeProperties.TryGetValue(parameterName, out var value) ? value : null;
        }

        public void SetTypeParameter(String parameterName, Object type) => _typeProperties.Add(parameterName, type);

        public Object GetTypeParameter(String parameterName) => _typeProperties.TryGetValue(parameterName, out var value) ? value : null;
    }

    [Test(objectParameter: 99, typeParameter: typeof(Int32), arrayParameter: new Char[] { 'd', 'e', 'f', 'g' }, StringProperty = "Property Assigned String Value 2", TypeProperty = typeof(Decimal))]
    internal class TestClass
    {
        private const String Prefix = "Prefixed";
        private const String ConstantField = Prefix + nameof(ConstantField);
        public void TestMethod()
        {

        }
    }

    internal class Program
    {
        private const String SOURCE =
@"using System;
using System.Collections.Generic;
namespace TestApp
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

        private const String TESTCLASS_SOURCE =
@"using System;
using TestApp;

namespace TestNamespace
{
	[Test(objectParameter: 99, arrayParameter: new char[] { 'd', 'e', 'f', 'g' }, stringParameter : ""Property Assigned String Value 2"", typeParameter: typeof(int))]
	internal class TestClass
	{
		private const string Prefix = ""Prefixed"";
		private const string ConstantField = Prefix + nameof(ConstantField);
		public void TestMethod()
		{

		}
	}

    internal class Program
    {
        public static void Main(string[] args)
        {
            var t = 0;
            t = t;

            throw new Exception();
            t = t;
		}
	}
}";

        private static AttributeAnalysisUnit<TestAttribute> AnalysisUnit { get; } = new AttributeAnalysisUnit<TestAttribute>(SOURCE);

        private static void Main(String[] args)
        {
            var compilation = CSharpCompilation.Create("TestAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(String).Assembly.Location))
                .AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(TESTCLASS_SOURCE),
                    CSharpSyntaxTree.ParseText(AnalysisUnit.GeneratedType.Source.Text));

            var type = compilation.SyntaxTrees.Select(t => t.GetRoot())
                .SelectMany(t => t.DescendantNodes(n => n is not BaseTypeDeclarationSyntax))
                .OfType<BaseTypeDeclarationSyntax>()
                .ToArray()[0];

            using var peStream = new MemoryStream();
            var result = compilation.Emit(peStream);

            var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);

            var attribute = type.AttributeLists.SelectMany(al => al.Attributes).OfAttributeClasses(semanticModel, AnalysisUnit.GeneratedType.Identifier).Single();

            _ = AnalysisUnit.Factory.TryBuild(attribute, semanticModel, out var attributeInstance);

            Console.WriteLine(attributeInstance?.GetTypeProperty(nameof(TestAttribute.TypeProperty)) ?? "null");
            Console.WriteLine((Object)attributeInstance?.TypeProperty ?? "null");
        }
    }
}