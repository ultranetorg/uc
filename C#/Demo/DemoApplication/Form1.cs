using System.Reflection;
using System;

namespace DemoApplication
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			InitializeComponent();


			//AppDomain.CurrentDomain.ExecuteAssembly()

			//AppDomainSetup.ApplicationBase = Path.Combine(AppDomainSetup.ApplicationBase, "..\\..");
			//var myAssembly = Assembly.LoadFrom(Path.Join(appDir, "Component.dll"));

		}

		private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
		{
			throw new NotImplementedException();
		}
	}
}