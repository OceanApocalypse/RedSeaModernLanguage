using System;
using System.Linq;
using System.Reflection;

namespace OceanApocalypse.RSML.CLI;

internal sealed class Program
{
	private Program() { }

	private static void Main(string[] args)
	{
		// todo: implement

		Console.WriteLine("Assembly Version: " +
			(typeof(Program).Assembly
				.GetName()
				.Version
				?.ToString()
			)
		);
		Console.WriteLine("File Version: " +
			(typeof(Program).Assembly
				.GetCustomAttribute<AssemblyFileVersionAttribute>()
				?.Version
			)
		);
		Console.WriteLine("Informational Version: " +
			(typeof(Program).Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion
			)
		);
		Console.WriteLine("Semantic Version: " +
			(typeof(Program).Assembly
				.GetCustomAttributes<AssemblyMetadataAttribute>()
				.FirstOrDefault(a => a.Key == "SemVersion")
				?.Value
			)
		);
	}
}
