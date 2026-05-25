using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

static class Program
{
	[STAThread]
	static void Main()
	{
		System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
		System.Windows.Forms.Application.EnableVisualStyles();
		System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

		Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
	}

	public static void ResizeColumnsToFit(this ListView listView)
    {
		if (listView.Columns.Count == 0) 
			return;

		listView.SuspendLayout();

        listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        int[] contentWidths = new int[listView.Columns.Count];
        for (int i = 0; i < listView.Columns.Count; i++)
        {
            contentWidths[i] = listView.Columns[i].Width;
        }

        listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        // 3. Compare, find the max required width, and apply ONLY if it changed
        for (int i = 0; i < listView.Columns.Count; i++)
        {
            int idealWidth = Math.Max(contentWidths[i], listView.Columns[i].Width);

            if(listView.Columns[i].Width < idealWidth)
            {
                listView.Columns[i].Width = idealWidth;
            }
        }

		listView.ResumeLayout();
    }
}
