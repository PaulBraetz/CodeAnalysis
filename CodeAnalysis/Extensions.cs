using Microsoft.CodeAnalysis;

namespace RhoMicro.CodeAnalysis
{
	internal static class Extensions
	{

		public static void AddSource(this GeneratorPostInitializationContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Source);
		}
		public static void AddSource(this GeneratorExecutionContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Source);
		}
	}
}
