using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public partial class TextForm : Form
	{

		public TextForm()
		{
			InitializeComponent();
		}

		public static void ShowDialog(string title, string message, string text)
		{
			var f = new TextForm();

			f.Text = title;
			f.message.Text = message;
			f.text.Text = text;

			f.ShowDialog();
		}

		private void send_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
