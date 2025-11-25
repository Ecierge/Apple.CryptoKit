using System.Reflection;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using System.Runtime.InteropServices;
using ObjCRuntime;

public class TestsEntryPoint : iOSApplicationEntryPoint
{
    protected override int? MaxParallelThreads => 1;

    protected override IDevice? Device => null;

    protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
    {
        yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
    }

    protected override void TerminateWithSuccess()
    {
        // Don't call Environment.Exit() - XHarness handles process termination
        // For MacCatalyst test apps, XHarness monitors test completion and kills the process
        Console.WriteLine("TerminateWithSuccess called - letting XHarness handle termination");
        Console.Out.Flush();
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var entryPoint = new TestsEntryPoint();
            await entryPoint.RunAsync();

            // Should not reach here - TerminateWithSuccess() should exit first
            Console.WriteLine("Unexpected: RunAsync completed without exiting");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test execution failed: {ex}");
            Console.Out.Flush();
            Environment.Exit(1);
            return 1;
        }
    }
}
