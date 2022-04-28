using System;
using System.Windows.Forms;

namespace UC.Net.Node.FUI
{
	public partial class PublishPanel : MainPanel
	{
		public PublishPanel(Core d, Vault vault) : base(d, vault)
		{
			InitializeComponent();

			Manifest.Text = 
@"Author = uo
Product = ousprototype
Version = 0.0.0.1
Build = Release
Platform
{
	Name = Windows
	Version = 6.1.[1511-21H2].*
	Architecture = x64
	Features = ''
}

Complete
{
	Core { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk ftp://company.com/releases/app-win-x64-0.0.0.1 }
	Deferred { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk http://company.com/releases/addon-win-x64-0.0.0.1 }
	Deferred { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk { Language = de-de }}
	Deferred { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk { Language = ru-ru }}
	Deferred { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk { Language = zh-cn }}
	Deferred { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk { Language = hi-in }}
}

Incremental
{
	Core { ipfs://qmcslmfa44cmtkmegdchz4hcylx8klnszxpwvkq2tkmenk }
}
";
		}

		public override void Open(bool first)
		{
			if(first)
			{
				BindAccounts(Account);
			}
		}

		private void Publish_Click(object sender, EventArgs e)
		{
			//var signer = GetPrivate(Account.SelectedItem as PrivateAccount);
			//var feeacker = new FeeForm(Core.Settings);
			//
			//if(feeacker.Ask(Account.SelectedItem as PrivateAccount, Core.fee))
			//{
			//	Core.Enqueue(new Release(Core.Chain, signer, Manifest.Text, feeacker.Fee));
			//}
		}
	}
}
