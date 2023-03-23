using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nethereum.KeyStore.Crypto;

namespace UC.Sun.FUI
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
		protected readonly Core		Core;
		protected readonly Vault	Vault;
		protected Database		Database => Core.Database;

		public BaseControl()
		{
		}

		public BaseControl(Core d, Vault v)
		{
			Core = d;
			Vault = v;
		}

		public IEnumerable<AuthorEntry> FindAuthors(AccountAddress owner)
		{
			var o = new List<AuthorEntry>();
			
			foreach(var r in Database.Tail)
				foreach(var a in r.AffectedAuthors)
					if(a.Value.Owner == owner && !o.Any(i => i.Name == a.Key))
					{
						o.Add(a.Value);
					}

			/// TODO: too slow
			o.AddRange(Database.Authors.Where(i => i.Owner == owner));

			return o;
		}

		public IEnumerable<ProductModel> FindProducts(AccountAddress owner)
		{
			var o = new List<ProductModel>();
			
			foreach(var r in Database.Tail)
				foreach(var p in r.AffectedProducts)
				{
					foreach(var rx in Database.Tail)
						foreach(var a in rx.AffectedAuthors)
							if(a.Value.Owner == owner && p.Key.Author == a.Key && !o.Any(i => i.Author.Name == a.Key && i.Product.Address == p.Key))
							{
								o.Add(new ProductModel{Product = p.Value, Author = a.Value});
							}
				}

			/// TODO: too slow
			foreach(var i in Database.Authors.Where(i => i.Owner == owner)
											 .SelectMany(a => Database.Products.Where(i => i.Address.Author == a.Name).Select(p =>	new ProductModel{Product = p, Author = a})))
			{
					if(!o.Any(x => x.Author.Name == i.Author.Name && x.Product.Address == i.Product.Address))
					{
						o.Add(i);
					}
			}

			return o;
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

// 		public void BindProducts(ComboBox b)
// 		{
// 			void fill()
// 			{
// 				b.Items.Clear();
// 	
// 				lock(Core.Lock)
// 				{
// 					foreach(var a in Vault.Accounts)
// 						foreach(var p in FindProducts(a))
// 							b.Items.Add(p.Product);
// 				}
// 	
// 				if(b.Items.Count > 0)
// 					b.SelectedIndex = 0;
// 			}
// 
// 			Core.Database.BlockAdded +=	b =>{
// 												if(b is Payload p && p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is ProductRegistration)))
// 												{
// 													BeginInvoke((MethodInvoker)delegate{ fill(); });
// 												}
// 											};
// 			fill();
// 		}
 
// 		public void BindAuthors(ComboBox b, Action filled = null)
// 		{
// 			void fill()
// 			{
// 				b.Items.Clear();
// 	
// 				lock(Core.Lock)
// 				{
// 					foreach(var i in Vault.Accounts)
// 						foreach(var p in FindAuthors(i))
// 							b.Items.Add(p.Name);
// 				}
// 	
// 				if(b.Items.Count > 0)
// 					b.SelectedIndex = 0;
// 			
// 				filled?.Invoke();
// 			}
// 
// 			Core.Database.BlockAdded += b => {
// 													if(b is Payload p && (	p.Transactions.Any(i => Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorRegistration || o is AuthorTransfer)) ||
// 																			p.Transactions.Any(i => !Vault.Accounts.Contains(i.Signer) && i.Operations.Any(o => o is AuthorTransfer at && Vault.Accounts.Contains(at.To))) 
// 																			))
// 													{
// 														BeginInvoke((MethodInvoker)delegate{ fill(); });
// 													}
// 												};
// 			fill();
// 		}

		public void ShowException(string message, Exception ex)
		{
			MessageBox.Show(this, message + " (" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void ShowError(string message)
		{
			MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public AccountKey GetPrivate(AccountAddress account)
		{
			var p = Vault.GetKey(account);
			
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

		public static string Dump(XonDocument doc)
		{
			string t = "";
			doc.Dump((n, l) => t += new string(' ', l * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value))) + Environment.NewLine);
			return t;
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
