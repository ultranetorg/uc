using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class MembershipPanel : MainPanel
	{
		public MembershipPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAccounts(Candidates);
				BindAccounts(NewCandidate);

				UpdateCandidateAccounts();

				Bail.Coins = Roundchain.BailMin;
				IP.Text = Core.IP.ToString();
			}
		}

		public override void Close()
		{
		}

		void UpdateCandidateAccounts()
		{
			FillAccounts(NewCandidate);

			if(Core.Settings.Generator != null)
			{
				lock(Core.Lock)
				{
					CurrentCandidate.Text = Core.Settings.Generator;
					NewCandidate.Items.Remove(PrivateAccount.Parse(Core.Settings.Generator));
	
					if(NewCandidate.Items.Count > 0)
						NewCandidate.SelectedIndex = 0;
				}
			}
			else
				CurrentCandidate.Text = null;
		}

		public override void PeriodicalRefresh()
		{
			Declarations.Items.Clear();
			Blocks.Items.Clear();

			lock(Core.Lock)
			{
				foreach(var i in Vault.Accounts)
				{
					foreach(var o in Core.Chain.Accounts.FindLastOperations<CandidacyDeclaration>(i))
					{
						var r = new ListViewItem(o.Signer.ToString());
						r.SubItems.Add(o.Transaction.Payload.RoundId.ToString()); 
						r.SubItems.Add(o.Bail.ToString()); 
	
						Declarations.Items.Add(r);
					}
	
					foreach(var b in Core.Chain.FindLastBlocks(j => j.Member == i).OrderBy(i => i.RoundId))
					{
						var r = new ListViewItem(b.RoundId.ToString());
						r.SubItems.Add(b.Type.ToString()); 
						r.SubItems.Add(b is Payload p ? p.Transactions.Count.ToString() : null); 
						r.SubItems.Add(b.Round.Time.ToString()); 
	
						Blocks.Items.Add(r);
					}
				}
			}

			Activate.Enabled = NewCandidate.Items.Count > 0/* && NewCandidate.SelectedItem as PrivateAccount != Core.Member*/; 
			Deactivate.Enabled = Core.Generator != null;
		}

		private void Declare_Click(object sender, EventArgs e)
		{
			try
			{
				Core.Enqueue(new CandidacyDeclaration(GetPrivate(Candidates.SelectedItem as Account), 
															Bail.Coins,
															IPAddress.Parse(IP.Text)));
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}

		private void Activate_Click(object sender, EventArgs e)
		{
			try
			{
				//var pf = new PasswordForm(Core.Settings.Secret?.Password);
				var key = GetPrivate(NewCandidate.SelectedItem as Account);
	
				if(key != null)
				{
					lock(Core.Lock)
					{
						Core.Settings.Generator = key.Key.GetPrivateKey();
						Core.Settings.Save();
						Core.Generator = key;
					}
	
					UpdateCandidateAccounts();
				}
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				ShowError(ex.Message);
			}
		}

		private void Deactivate_Click(object sender, EventArgs e)
		{
			try
			{
				lock(Core.Lock)
				{
					Core.Settings.Generator = null;
					Core.Settings.Save();
					Core.Generator = null;
				}
	
				UpdateCandidateAccounts();
			}
			catch(Exception ex)
			{
				ShowError(ex.Message);
			}
		}
	}
}
