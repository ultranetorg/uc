using System.Collections;
using System.Windows.Forms;

namespace Uccs.Mcv.FUI;

public class BaseControl : UserControl
{
	public BaseControl()
	{
	}

	public void ShowException(string message, Exception ex)
	{
		MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public void ShowError(string message)
	{
		MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	public static string Dump(Xon doc)
	{
		string t = "";
		doc.Dump((n, l) => t += new string(' ', l * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<string>(n, n.Value))) + Environment.NewLine);
		return t;
	}

	public void Dump(object o, Control fields, Control values)
	{
		void save(string name, Type type, object value, int tab)
		{
			fields.Text += new string(' ', tab * 4) + $"{name}\n";

			if(type.GetInterfaces().Any(i => i == typeof(ICollection)))
			{
				values.Text += $"{{{(value as ICollection)?.Count}}}\n";
			}
			else
			{
				values.Text += $"{value?.ToString()}\n";
			}
		}

		foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
		{
			save(i.Name, i.PropertyType, i.GetValue(o), 1);
		}
	}
}

public class McvPanel : BaseControl
{
	public bool First = true;

	public virtual void Open(bool first){ }
	public virtual void Close(){ }
	public virtual void PeriodicalRefresh(){ }

	public McvPanel()
	{
	}
}
