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
		private static readonly TypeIdentifierName TestAttributeIdentifierName = TypeIdentifierName.Create<TestAttribute>();
		private static readonly Namespace TestAttributeNamespace = Namespace.Create<TestAttribute>();
		private static readonly TypeIdentifier TestAttributeIdentifier = TypeIdentifier.Create(TestAttributeIdentifierName, TestAttributeNamespace);

		static void Main(string[] args)
		{
			//Console.WriteLine(String.Join("\n", Enumerable.Range(1, 1).Subsets().Select(s => $"[{String.Join(", ", s)}]")));
			//return;

			var compilation = CSharpCompilation.Create("TestAssembly")
				.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(TestClass));

			var type = compilation.SyntaxTrees.Select(t => t.GetRoot())
				.SelectMany(t => t.DescendantNodes(n => !(n is BaseTypeDeclarationSyntax)))
				.OfType<BaseTypeDeclarationSyntax>()
				.ToArray()[1];

			var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);

			var attribute = type.AttributeLists.SelectMany(al => al.Attributes).OfAttributeClasses(semanticModel, TypeIdentifier.Create<TestAttribute>()).Single();

			IAttributeFactory<TestAttribute> factory = AttributeFactory<TestAttribute>.Create();

			factory.TryBuild(attribute, semanticModel, out TestAttribute attributeInstance);
		}

		private const String TestClass =
@"namespace TestApp
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class TestAttribute : Attribute
	{
		public TestAttribute()
		{
		}
		public TestAttribute(char[] arrayParameter, object objectParameter, string stringParameter = ""Default Value"")
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

		public object ObjectProperty { get; set; } = ""DefaultObjectValue"";
		public char[] ArrayProperty { get; set; } = new char[] { 'a', 'b', 'c' };
		public string StringProperty { get; set; } = ""DefaultPropertyValue"";
	}

	[Test(/*new char[] { 'd', 'e', 'f', 'g' },*/ ObjectProperty= new object(), StringProperty = ""Property Assigned String Value"")]
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