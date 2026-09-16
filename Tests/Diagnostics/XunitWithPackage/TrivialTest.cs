using System.Runtime.CompilerServices;

namespace XunitWithPackage;

/// <summary>
/// The same trivial test again, so the only difference from MinimalXunit is the package reference.
/// </summary>
/// <remarks>
/// Deliberately does not use WebRTCme. The question is whether having the package present is enough
/// to stop the process starting - not whether calling it works, which the scenarios already answer
/// everywhere else.
/// </remarks>
public class TrivialTest
{
    [ModuleInitializer]
    internal static void Announce()
    {
        Console.Error.WriteLine("WITHPACKAGE | assembly loaded on " + Environment.OSVersion);
        Console.Error.Flush();
    }

    [Fact]
    public void True_is_true() => Assert.True(true);
}
