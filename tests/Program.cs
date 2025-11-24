using System.Reflection;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using System.Runtime.InteropServices;
using ObjCRuntime;

public class TestsEntryPoint : ApplicationEntryPoint
{
    protected override bool IsXunit => false;

    protected override int? MaxParallelThreads => 1;

    protected override IDevice? Device => null;

    protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
    {
        yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
    }

    protected override TestRunner GetTestRunner(LogWriter logWriter)
    {
        // Try to explicitly load the NUnit runner assembly
        Assembly? nunitAssembly = null;

        // First check if already loaded
        nunitAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Microsoft.DotNet.XHarness.TestRunners.NUnit");

        // If not loaded, try to load it explicitly
        if (nunitAssembly == null)
        {
            try
            {
                nunitAssembly = Assembly.Load("Microsoft.DotNet.XHarness.TestRunners.NUnit");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load NUnit runner assembly: {ex.Message}");
                Console.WriteLine($"Loaded assemblies: {string.Join(", ", AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name))}");
                throw new InvalidOperationException("Could not find Microsoft.DotNet.XHarness.TestRunners.NUnit assembly", ex);
            }
        }

        var nunitRunnerType = nunitAssembly.GetType("Microsoft.DotNet.XHarness.TestRunners.NUnit.NUnitTestRunner");
        if (nunitRunnerType == null)
        {
            throw new InvalidOperationException("Could not find NUnitTestRunner type");
        }

        return (TestRunner)Activator.CreateInstance(nunitRunnerType, logWriter)!;
    }

    protected override void TerminateWithSuccess()
    {
        // Don't call Environment.Exit() - XHarness handles process termination
        // For MacCatalyst test apps, XHarness monitors test completion and kills the process
        Console.WriteLine("TerminateWithSuccess called - letting XHarness handle termination");
        Console.Out.Flush();
    }

    // Implement RunAsync - required by ApplicationEntryPoint
    public override async Task RunAsync()
    {
        // Use ApplicationOptions to get configuration
        var options = ApplicationOptions.Current;

        // Simplified implementation that avoids Environment.GetFolderPath() and internal TcpTextWriter
        var logger = new LogWriter(Device, Console.Out);
        logger.MinimumLogLevel = MinimumLogLevel;

        var runner = GetTestRunner(logger);
        runner.LogExcludedTests = false;

        var testAssemblies = GetTestAssemblies();
        await runner.Run(testAssemblies).ConfigureAwait(false);

        Console.WriteLine("Test execution completed");

        // Write test summary
        logger.Info($"{Environment.NewLine}=== TEST EXECUTION SUMMARY ==={Environment.NewLine}Tests run: {runner.TotalTests} Passed: {runner.PassedTests} Inconclusive: {runner.InconclusiveTests} Failed: {runner.FailedTests} Ignored: {runner.FilteredTests} Skipped: {runner.SkippedTests}{Environment.NewLine}");

        // Write app end tag if configured - this signals XHarness that tests are complete
        if (options.AppEndTag != null)
        {
            logger.Info(options.AppEndTag);
        }

        Console.Out.Flush();

        // Call TerminateWithSuccess - but it won't actually exit for MacCatalyst
        if (options.TerminateAfterExecution)
        {
            TerminateWithSuccess();
        }
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
