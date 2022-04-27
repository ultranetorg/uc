using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Nethereum.Util;

namespace UC.Net.Node.FUI
{
	public class CoinEdit : TextBox
	{
		private string currentText;

		public CoinEdit()
		{
			TextAlign = HorizontalAlignment.Right;
			Text = "0.000000";
		}

		public Coin Coins
		{
			get
			{
				return string.IsNullOrWhiteSpace(Text) ? Coin.Zero : new Coin(decimal.Parse(Text));
			}
			set
			{
				Text = value.ToDecimal().ToString();
			}
		}

		public BigInteger Wei
		{
			get
			{
				return string.IsNullOrWhiteSpace(Text) ? BigInteger.Zero : Nethereum.Web3.Web3.Convert.ToWei(BigDecimal.Parse(Text));
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if(!char.IsNumber(e.KeyChar) & (Keys)e.KeyChar != Keys.Back & e.KeyChar != '.')
			{
				e.Handled = true;
			}

			base.OnKeyPress(e);
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if(this.Text.Length > 0)
			{
				decimal result;

				bool isNumeric = decimal.TryParse(this.Text, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out result);

				if(isNumeric)
				{
					currentText = this.Text;
				}
				else
				{
					this.Text = currentText;
					this.Select(this.Text.Length, 0);
				}
			}

			base.OnTextChanged(e);
		}
	}
}
