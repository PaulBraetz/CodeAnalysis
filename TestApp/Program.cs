using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Attributes;
using System;
using System.ComponentModel;
using System.Reflection;

namespace TestApp
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TestAttribute : Attribute
	{
		public TestAttribute(String stringParameter = "DefaultParameterValue")
		{

		}
		public TestAttribute(Char[] arrayParameter = null)
		{

		}
		public TestAttribute(Object objectParameter = null)
		{

		}

		public String StringProperty { get; set; } = "DefaultPropertyValue";
	}

	[Test(objectParameter: null)]
	[Test(arrayParameter: new Char[] { 'a', 'b' })]
	[Test(stringParameter: "ab")]
	public class TestClass
	{
		private const string ConstantField = "ConstantValue";
		public void TestMethod()
		{

		}
	}
	public class Program
	{
		private static readonly TypeIdentifierName TestAttributeIdentifierName = TypeIdentifierName.CreateAttribute<TestAttribute>();
		private static readonly Namespace TestAttributeNamespace = Namespace.Create<TestAttribute>();
		private static readonly TypeIdentifier TestAttributeIdentifier = TypeIdentifier.Create(TestAttributeIdentifierName, TestAttributeNamespace);

		private static readonly AttributeParameterDefinition StringDefinition = new AttributeParameterDefinition(null, null, 0, false);
		private static readonly AttributeParameter<String> StringParameter = new AttributeParameter<String>(new[] { StringDefinition });

		private static readonly AttributeParameterDefinition Int32Definition = new AttributeParameterDefinition(null, null, 1, false);
		private static readonly AttributeParameter<Int32> Int32Parameter = new AttributeParameter<Int32>(new[] { Int32Definition });

		private static readonly AttributeParameterDefinition TypeDefinition = new AttributeParameterDefinition(null, null, 2, false);
		private static readonly TypeIdentifierAttributeParameter TypeParameter = new TypeIdentifierAttributeParameter(new[] { TypeDefinition });

		private static readonly AttributeParameterDefinition ArrayDefinition = new AttributeParameterDefinition(null, null, 3, false);
		private static readonly ArrayAttributeParameter<Byte> ArrayParameter = new ArrayAttributeParameter<Byte>(ArrayDefinition);

		private static readonly AttributeParameterDefinition ObjectDefinition = new AttributeParameterDefinition(null, null, 4, false);
		private static readonly AttributeParameter<Object> ObjectParameter = new AttributeParameter<Object>(ObjectDefinition);

		static void Main(string[] args)
		{
			var compilation = CSharpCompilation.Create("TestAssembly")
				.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(TestClass));

			var definition = new AttributeDefinition(TestAttributeIdentifier, StringParameter, Int32Parameter, TypeParameter, ArrayParameter, ObjectParameter);

			var typeDeclaration = CompilationAnalysis.GetTypeDeclarations(compilation, new[] { TestAttributeIdentifier }).Single();

			var declaration = CompilationAnalysis.GetAttributes(typeDeclaration.AttributeLists, typeDeclaration, compilation, new[] { definition }).Single();

			var stringArg = declaration.GetArgument<String>(StringDefinition);
			Console.WriteLine(stringArg);

			var intArg = declaration.GetArgument<Int32>(Int32Definition);
			Console.WriteLine(intArg);

			var typeArg = declaration.GetArgument<TypeIdentifier>(TypeDefinition);
			Console.WriteLine(typeArg);

			var arrayArg = declaration.GetArgument<Byte[]>(ArrayDefinition);
			Console.WriteLine(arrayArg);

			var objectArg = declaration.GetArgument<Object>(ObjectDefinition);
			Console.WriteLine(objectArg);
		}

		private const String TestClass =
@"using TestApp;
namespace TestNamespace
{
	[Test(ConstantField,32,typeof(uint), new byte[] {(byte)2, (byte)4, (byte)6}, null)]
	public class TestClass
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