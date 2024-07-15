using System.Globalization;
using System.Numerics;
using System.Windows.Forms;

namespace Uccs.Net.FUI
{
	public class CoinEdit : TextBox
	{
		private string currentText;

		public CoinEdit()
		{
			TextAlign = HorizontalAlignment.Right;
			Text = "0.000000";
		}

		public Money Coins
		{
			get
			{
				return string.IsNullOrWhiteSpace(Text) ? Money.Zero : Money.Parse(Text);
			}
			set
			{
				Text = value.ToString();
			}
		}

		public BigInteger Wei
		{
			get
			{
				return string.IsNullOrWhiteSpace(Text) ? BigInteger.Zero : Money.Parse(Text).Attos;
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
