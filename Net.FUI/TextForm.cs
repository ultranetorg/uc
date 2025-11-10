using System.Windows.Forms;

namespace Uccs.Net.FUI;

public partial class TextForm : Form
{
	public TextForm()
	{
		InitializeComponent();
	}

	public static void ShowDialog(IWin32Window owner, string title, string message, string text)
	{
		var f = new TextForm();

		int d = 0;
		int x = 0;

		f.Text = title;
		
		var h = f.message.Height;
		f.message.Text = message;
		d += f.message.Height - h;

		f.text.Top += d;
		h = f.text.Height;
		f.text.Text = text;
		d += f.text.Height - h;

		f.send.Top += d;


		f.PerformAutoScale();
		
		//f.Height += d;

		f.ShowDialog(owner);
	}

	public static DialogResult ShowDialog(IWin32Window owner, string title, string message, out string text)
	{
		var f = new TextForm();
		f.DialogResult = DialogResult.Cancel;

		int d = 0;
		int x = 0;

		f.Text = title;
		
		var h = f.message.Height;
		f.message.Text = message;
		d += f.message.Height - h;

		f.text.Top += d;
		f.text.ReadOnly = false;

		f.send.Top += d;

		f.PerformAutoScale();
		
		//f.Height += d;

		var r = f.ShowDialog(owner);

		text = f.text.Text;

		return r;
	}

	private void OK_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}
}
