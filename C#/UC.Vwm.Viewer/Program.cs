using System;
using System.Collections.Generic;
using System.Windows.Forms;

//copy /y "$(ProjectDir)..\Libs\DevIL\lib\unicode\DevIL.dll" "$(TargetDir)\DevIL.dll" 
//copy /y "$(ProjectDir)..\Libs\CRT\Debug\ucrtbased.dll" "$(TargetDir)\ucrtbased.dll"

namespace UC.Vwm.Viewer
{
	public static class Program
	{
		public static string[] _args;
	
		[STAThread]
		public static void Main(string [] args)
		{
			_args = args;
					
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}