using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Attributes;
using System;
using System.Linq;

namespace TestApp
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class TestAttribute : Attribute
	{
		public TestAttribute()
		{
		}
		public TestAttribute(char[] arrayParameter, object objectParameter, string stringParameter = "Default Value")
		{
			ObjectProperty = objectParameter;
			ArrayProperty = arrayParameter;
			StringProperty = stringParameter;
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
	}

	internal class Program
	{
		private static AttributeAnalysisUnit<TestAttribute> AnalysisUnit { get; } = new AttributeAnalysisUnit<TestAttribute>("namespace TestApp{[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]internal class TestAttribute : Attribute{public TestAttribute(){}public TestAttribute(char[] arrayParameter, object objectParameter, string stringParameter = \"Default Value\"){ObjectProperty = objectParameter;ArrayProperty = arrayParameter;StringProperty = stringParameter;}public TestAttribute(object objectParameter, char[] arrayParameter, string stringParameter){ObjectProperty = objectParameter;ArrayProperty = arrayParameter;StringProperty = stringParameter;}\r\npublic object ObjectProperty { get; set; } = \"DefaultObjectValue\";public char[] ArrayProperty { get; set; } = new char[] { 'a', 'b', 'c' };public string StringProperty { get; set; } = \"DefaultPropertyValue\";}}");

		static void Main(string[] args)
		{
			//Console.WriteLine(String.Join("\n", Enumerable.Range(1, 1).Subsets().Select(s => $"[{String.Join(", ", s)}]")));
			//return;

			var compilation = CSharpCompilation.Create("TestAssembly")
				.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(TestClass), CSharpSyntaxTree.ParseText(AnalysisUnit.GeneratedType.Source.Text));

			var type = compilation.SyntaxTrees.Select(t => t.GetRoot())
				.SelectMany(t => t.DescendantNodes(n => !(n is BaseTypeDeclarationSyntax)))
				.OfType<BaseTypeDeclarationSyntax>()
				.ToArray()[0];

			var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);

			var attribute = type.AttributeLists.SelectMany(al => al.Attributes).OfAttributeClasses(semanticModel, AnalysisUnit.GeneratedType.Identifier).Single();

			AnalysisUnit.Factory.TryBuild(attribute, semanticModel, out TestAttribute attributeInstance);
		}

		private const String TestClass =
@"namespace TestNamespace
{
	[TestApp.Test(objectParameter: 99, StringProperty = ""Property Assigned String Value 2"", arrayParameter: new char[] { 'd', 'e', 'f', 'g' })]
	internal class TestClass
	{
		private const string Prefix = ""Prefixed"";
		private const string ConstantField = Prefix + nameof(ConstantField);
		public void TestMethod()
		{

		}
	}
}";
	}
}