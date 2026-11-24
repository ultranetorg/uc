using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommonUtils.Ext;
using JunitReporter;
using O;
using TestAutomation.Tests;
using TestAutomation.Tests.Fair;
using TestAutomation.Tests.Rdn;


namespace TestAutomation;

// UO_TA_LOGECHO
// "all" - print called commands and their outputs
// "input" - print only called commands

// TODO : Need to introduce some kind of generic test method to run after each particular test was ran,
//		  to ensure that network did not crash.


/// <summary>
/// This is a test launcher. Classes that represent tests should indirectly inherit 
/// 'TestBase' class to be launched as a tests. Using reflection we get all these test 
/// classes, one by one, we instantiate them and run their `Init()` and then `Logic()` 
/// methods. `Init()` of these test classes acts like a test preparation step, where
/// we set-up test data and conditions. This approach lets us isolate tests very well 
/// and have an ability to set-up a test context easily. It also allows to structure 
/// project nicely. Tests can have different "types" defined in the 'TestType' enum 
/// located inside of 'TestAutomation' namespace. You can assign a tests type to a test 
/// by adding a TestTypeAttribute to the test class, e.g. Right now there are "Generic", 
/// "CommandLine", "EndToEnd" and "TestSet" test types, but there could be more in future 
/// or they can change.
/// <br/> <br/>
/// 
/// Example. <br/>
/// Here we define a test class `AccountUtilityTransfer` that tests utility transfer 
/// functionality:
///  <br/>  <br/>
///  
/// [TestType(TestType.CommandLine)]			<br/> 
/// class AccountUtilityTransfer : FairTest		<br/>
/// {											<br/>
/// 	public override void Init() { .. }		<br/>
/// 	public override void Logic() { .. }		<br/>
/// }											<br/>
/// 
/// <br/><br/>
/// 
/// We add a test type attribute with a `TestType.CommandLine` argument, marking this test 
/// as a command line test, meaning it tests functionality through command line. We inherit 
/// our test from `FairTest` class, that in turn inherits 'TestBase' class. Our test 
/// launcher detects everything that indirectly inherits 'TestBase' as a test. We implement 
/// `Init()` and `Logic()` methods that our test launcher expectes test classes to have.
/// </summary>
class Program
{
	//static List<Type> Only = [];
	static List<Type> Only = [typeof(FeeValidation)];
	static List<Type> TestsToSkip = [];

	static int TimeoutSec = 60;

	/// <summary> A test class together with the values of its [TestType] and [TestNetwork] attributes. </summary>
	record TestInfo(Type Class, TestType TestType, TestNetwork Network);

	/// <summary> Finds all test classes: non-abstract classes that indirectly inherit `TestBase`. </summary>
	static List<TestInfo> FindTests(Assembly assembly)
	{
		List<TestInfo> tests = [];

		foreach(Type t in assembly.GetTypes())
		{
			if(!t.IsSubclassOf(typeof(TestBase)) || t.IsAbstract)
				continue;

			TestTypeAttribute type = t.GetCustomAttribute<TestTypeAttribute>()
				?? throw new Exception($"Test class '{t.Name}' has no [TestType] attribute.");

			TestNetworkAttribute network = t.GetCustomAttribute<TestNetworkAttribute>()
				?? throw new Exception($"Test class '{t.Name}' has no [TestNetwork] attribute.");

			tests.Add(new TestInfo(t, type.TestType, network.TestNetwork));
		}

		return tests;
	}

	static void PrintGroup(string groupName, IEnumerable<TestInfo> tests)
	{
		Console.WriteLine($"{tests.Count()} {groupName} tests:");
		Console.WriteLine(string.Join(' ', tests.Select(t => t.Class.Name)));
		Console.WriteLine();
	}

	static void Main(string[] args)
	{
		// Pull test step execution timeout from env variable, if it is provided.
		string? timeout = Environment.GetEnvironmentVariable("UO_TA_TIMEOUT");
		if(timeout != null)
			int.TryParse(timeout, out TimeoutSec);

		/* Ultranet network nodes and vault needs a place to store it's data.
		It is stored in the `G.Developer.Tmp` path, so before we run new 
		iteration of tests - we want to clean old data to prevent conflicts. */
		new DirectoryInfo(G.Developer.Tmp).Empty();
		Assembly a = Assembly.GetAssembly(typeof(Program)) ?? throw new Exception("Could not acquaire current assembly");

		List<TestInfo> tests = FindTests(a);

		// Print which tests were found, by test type and by network.
		foreach(var group in tests.GroupBy(t => t.TestType))
			PrintGroup(group.Key.ToString(), group);

		foreach(var group in tests.GroupBy(t => t.Network))
			PrintGroup(group.Key.ToString(), group);

		if(!SelectTests(args, tests))
		{
			Environment.ExitCode = 1;
			return;
		}

		List<TestInfo> toRun = tests.Where(t => (Only.Count == 0 || Only.Contains(t.Class)) && !TestsToSkip.Contains(t.Class)).ToList();
		PrintGroup("selected", toRun);

		// Run tests, one suite per test type.
		DateTime startTime = DateTime.Now;
		foreach(var group in toRun.GroupBy(t => t.TestType).OrderBy(g => g.Key))
		{
			Reporter.BeginSuite(group.Key.ToString());

			foreach(TestInfo test in group)
				RunTest(test.Class);
		}
		TimeSpan time = DateTime.Now - startTime;
		Console.WriteLine($"TOTAL TIME: {time}");

		Reporter.Write("Result.xml");
	}

	/// <summary>
	/// Fills `Only` and `TestsToSkip` from command line arguments or environment variables.
	/// <br/><br/>
	/// Command line: `TestAutomation [Test1 Test2 ...] [--skip Test3 Test4 ...]` <br/>
	/// Names before `--skip` are the tests to run, names after it are the tests to skip.
	/// <br/><br/>
	/// Environment variables (space separated names): `UO_TA_ONLY`, `UO_TA_SKIP`.
	/// <br/><br/>
	/// For each list the first non-empty source wins: command line, then environment variable,
	/// then the hardcoded list in this file. A name is a test class name, short (`FeeValidation`)
	/// or full (`TestAutomation.Tests.Fair.FeeValidation`), case-insensitive.
	/// Returns `false` if an argument or a test name is not valid.
	/// </summary>
	static bool SelectTests(string[] args, List<TestInfo> tests)
	{
		List<string> onlyArgs = [];
		List<string> skipArgs = [];
		List<string> current = onlyArgs;

		foreach(string arg in args)
		{
			if(arg.Equals("--skip", StringComparison.OrdinalIgnoreCase))
				current = skipArgs;
			else if(arg.StartsWith('-'))
			{
				Console.Error.WriteLine($"Unknown option '{arg}'. Usage: TestAutomation [Test1 Test2 ...] [--skip Test3 Test4 ...]");
				return false;
			}
			else
				current.Add(arg);
		}

		static List<string> FromEnv(string name) =>
			Environment.GetEnvironmentVariable(name)?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [];

		if(onlyArgs.Count == 0) onlyArgs = FromEnv("UO_TA_ONLY");
		if(skipArgs.Count == 0) skipArgs = FromEnv("UO_TA_SKIP");

		bool ok = true;

		if(onlyArgs.Count != 0)
			ok &= ResolveTests(onlyArgs, tests, out Only);

		if(skipArgs.Count != 0)
			ok &= ResolveTests(skipArgs, tests, out TestsToSkip);

		if(!ok)
			Console.Error.WriteLine($"Available tests: {string.Join(' ', tests.Select(t => t.Class.Name).Order())}");

		return ok;
	}

	/// <summary> Maps test names to test classes. Returns `false` if any name is unknown or matches more than one test. </summary>
	static bool ResolveTests(List<string> names, List<TestInfo> tests, out List<Type> result)
	{
		result = [];
		bool ok = true;

		foreach(string name in names)
		{
			List<Type> matches = tests.Select(t => t.Class)
									  .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
												  c.FullName.Equals(name, StringComparison.OrdinalIgnoreCase))
									  .ToList();

			if(matches.Count == 1)
				result.Add(matches[0]);
			else
			{
				ok = false;
				Console.Error.WriteLine(matches.Count == 0 ? $"Unknown test '{name}'."
														   : $"Test name '{name}' is ambiguous: {string.Join(", ", matches.Select(m => m.FullName))}. Use the full name.");
			}
		}

		return ok;
	}

	static void RunTest(Type testClass)
	{
		TestBase test = (TestBase)Activator.CreateInstance(testClass);

		Reporter.BeginTest(test.Name, testClass.Name, test.Description);

		try
		{
			int timeout = Debugger.IsAttached ? int.MaxValue : TimeoutSec;
			bool first = RunTimed(test.Init, timeout);

			if(!first) throw new Exception($"Test initialization timeout. Init() execution time exceeded {timeout} seconds.");
		}
		catch(Exception e)
		{
			string msg = "Failed to initalize test class.";
			Reporter.Report(e, msg);
			test.Abort();

			return;
		}

		try
		{
			bool first = RunTimed(test.Logic, Debugger.IsAttached ? int.MaxValue : TimeoutSec);

			if(!first) throw new Exception($"Test execution timeout. Test execution time exceeded {TimeoutSec} seconds.");

			Reporter.Report(); // Test passed successfully without exceptions
		}
		catch(Exception e)
		{
			Reporter.Report(e, e.Message);
		}

		// This should kill uoses and their underliyng tcp listeners if any, and free all the locks/ports, etc.
		test.Abort();
	}

	/// <summary> Returns `true` if an action ended execution earlier than a timeout, `false` otherwise </summary>
	static bool RunTimed(Action action, int timeoutSec)
	{
		Task mainTask = Task.Run(action);
		Task<Task> race = Task.WhenAny(mainTask, Task.Delay(timeoutSec <= 0 ? int.MaxValue : (timeoutSec > int.MaxValue / 1000 ? int.MaxValue : timeoutSec * 1000)));
		race.Wait();
		if(race.Result != mainTask) return false;
		if(race.Result.IsFaulted) throw race.Result.Exception;
		return true;
	}
}

