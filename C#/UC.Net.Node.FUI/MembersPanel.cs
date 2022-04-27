using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class MembersPanel : MainPanel
	{
		public MembersPanel(Dispatcher d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			members.Items.Clear();

			lock(Dispatcher.Lock)
			{
				foreach(var i in Dispatcher.Chain.Members.OrderByDescending(i => i.JoinedAt))
				{
					var li = new ListViewItem(i.Generator.ToString());

					li.SubItems.Add(i.JoinedAt.ToString());
					li.SubItems.Add(Dispatcher.Chain.Accounts.FindLastOperation<CandidacyDeclaration>(i.Generator).Bail.ToHumanString());
					li.SubItems.Add(i.IP.ToString());

					members.Items.Add(li);
				}
			}
		}
	}
}
