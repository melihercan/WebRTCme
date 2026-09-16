using System.Runtime.CompilerServices;

namespace XunitWithTestSdk;

/// <summary>
/// The same trivial test as MinimalXunit, so that the only difference between the two projects is
/// Microsoft.NET.Test.Sdk and xunit.runner.visualstudio.
/// </summary>
public class TrivialTest
{
    [ModuleInitializer]
    internal static void Announce()
    {
        Console.Error.WriteLine("WITHTESTSDK | assembly loaded on " + Environment.OSVersion);
        Console.Error.Flush();
    }

    [Fact]
    public void True_is_true() => Assert.True(true);
}
