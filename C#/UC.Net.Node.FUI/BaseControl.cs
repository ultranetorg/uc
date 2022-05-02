using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nethereum.KeyStore.Crypto;

namespace UC.Net.Node.FUI
{
	public class ProductModel
	{
		public ProductEntry Product;
		public AuthorEntry Author;

		public ProductModel()
		{
		}
	}

	public class BaseControl : UserControl
	{
		protected readonly Core	Core;
		protected readonly Vault		Vault;
		protected Roundchain			Chain => Core.Chain;

		public BaseControl()
		{
		}

		public BaseControl(Core d, Vault v)
		{
			Core = d;
			Vault = v;
		}

		public IEnumerable<AuthorEntry> FindAuthors(Account account)
		{
			return Core.Chain.FindAuthors(account, Chain.LastConfirmedRound);
		}

		public IEnumerable<ProductModel> FindProducts(Account account)
		{
			return Chain.FindAuthors(account, Chain.LastConfirmedRound)
						.SelectMany(a => Chain.Authors.Find(a.Name, Chain.LastConfirmedRound.Id).Products.Select(i =>	new ProductModel
																														{
																															Product = Chain.FindProduct(new ProductAddress(a.Name, i.Name),  Chain.LastConfirmedRound.Id),
																															Author	= a
																														}));
		}

		protected void FillAccounts(ComboBox b)
		{
			b.Items.Clear();
	
			foreach(var i in Vault.Accounts)
				b.Items.Add(i);
		
			if(b.Items.Count > 0)
				b.SelectedIndex = 0;
		}

		public void BindAccounts(ComboBox b, Action filled = null)
		{
			Vault.AccountsChanged += () => {
												FillAccounts(b);
												filled?.Invoke();
											};
			FillAccounts(b);
			filled?.Invoke();
		}

		public void BindProducts(ComboBox b)
		{
			void fill()
			{
				b.Items.Clear();
	
				lock(Core.Lock)
				{
					foreach(var a in Vault.Accounts)
						foreach(var p in FindProducts(a))
							b.Items.Add(p.Product);
				}
	
				if(b.Items.Count > 0)
					b.SelectedIndex = 0;
			}

			Core.Chain.BlockAdded +=	b =>{
													if(b is Payload p && p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is ProductRegistration)))
													{
														BeginInvoke((MethodInvoker)delegate{ fill(); });
													}
												};
			fill();
		}

		public void BindAuthors(ComboBox b, Action filled = null)
		{
			void fill()
			{
				b.Items.Clear();
	
				lock(Core.Lock)
				{
					foreach(var i in Vault.Accounts)
						foreach(var p in FindAuthors(i))
							b.Items.Add(p.Name);
				}
	
				if(b.Items.Count > 0)
					b.SelectedIndex = 0;
			
				filled?.Invoke();
			}

			Core.Chain.BlockAdded += b => {
													if(b is Payload p && (	p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorRegistration || o is AuthorTransfer)) ||
																			p.Transactions.Any(i => !Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorTransfer at && Vault.Accounts.Contains(at.To))) 
																			))
													{
														BeginInvoke((MethodInvoker)delegate{ fill(); });
													}
												};
			fill();
		}

		public void ShowException(string message, Exception ex)
		{
			MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void ShowError(string message)
		{
			MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public PrivateAccount GetPrivate(Account account)
		{
			var p = Vault.GetPrivate(account);
			
			if(p != null)
			{
				return p;
			}

			var pa = new PasswordForm(Core.Settings.Secret?.Password);

			if(pa.Ask($"A password required to access {account} account"))
			{
				try
				{
					return Core.Vault.Unlock(account, pa.Password);
				}
				catch(Exception ex)
				{
					MessageBox.Show(this, $"Account access failed.\nThe password is incorrect or wallet file is invalid.\n({ex.Message})", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			return null;
		}
	}

	public class MainPanel : BaseControl
	{
		public bool First = true;

		public virtual void Open(bool first){ }
		public virtual void Close(){ }
		public virtual void PeriodicalRefresh(){ }

		public MainPanel()
		{
		}

		public MainPanel(Core core, Vault vault) : base(core, vault)
		{
		}
	}
}
