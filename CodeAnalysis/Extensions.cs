using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace RhoMicro.CodeAnalysis
{
	internal static class Extensions
	{

		public static void AddSource(this GeneratorPostInitializationContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Source);
		}
		public static void AddSources(this GeneratorPostInitializationContext context, IEnumerable<GeneratedSource> sources)
		{
			foreach (var source in sources)
			{
				context.AddSource(source);
			}
		}
		public static void AddSources(this GeneratorPostInitializationContext context, params GeneratedSource[] sources)
		{
			foreach (var source in sources)
			{
				context.AddSource(source);
			}
		}

		public static void AddSource(this GeneratorExecutionContext context, GeneratedSource source)
		{
			context.AddSource(source.HintName, source.Source);
		}
		public static void AddSources(this GeneratorExecutionContext context, IEnumerable<GeneratedSource> sources)
		{
			foreach(var source in sources)
			{
				context.AddSource(source);
			}
		}
		public static void AddSources(this GeneratorExecutionContext context, params GeneratedSource[] sources)
		{
			foreach (var source in sources)
			{
				context.AddSource(source);
			}
		}
	}
}
