using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RhoMicro.CodeAnalysis
{
	internal readonly struct Error
	{
		private Error(ImmutableArray<Exception> exceptions)
		{
			Exceptions = exceptions;

			_string =
$@"/*
An error has occured:
{String.Join("\n\n", exceptions.Select(e => e.ToString()))}
*/";
		}

		public readonly ImmutableArray<Exception> Exceptions;
		private readonly string _string;

		public static Error Create()
		{
			return new Error(ImmutableArray.Create<Exception>());
		}
		public Error With(Exception exception)
		{
			return new Error(Exceptions.Add(exception));
		}
		public Error With(params Exception[] exceptions)
		{
			return new Error(Exceptions.AddRange(exceptions));
		}

		public override String ToString()
		{
			return _string;
		}

		public static implicit operator String(Error error)
		{
			return error.ToString();
		}
	}
}
