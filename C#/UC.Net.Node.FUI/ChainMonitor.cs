using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Numerics;
using Nethereum.Web3;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Cms;
using System.Runtime.Intrinsics.X86;

namespace UC.Net.Node
{
	public partial class ChainMonitor : UserControl
	{
		Dictionary<Account, Brush>	Brushes = new Dictionary<Account, Brush>();
		Dictionary<Account, Pen>	Pens = new Dictionary<Account, Pen>();
		bool						Mode = false;

		static List<Coin[]>			stat;
		Coin emission = 0;
		BigInteger spent = 0;

		public Core Core;

		public ChainMonitor()
		{
			InitializeComponent();

			DoubleBuffered = true;
			Font = new Font("Lucida Console", 8.25F);
		}

		protected override void OnClick(EventArgs e)
		{
			Mode = !Mode;

			Invalidate();
		}

		void OnBlockAdded(Block b)
		{
			BeginInvoke((MethodInvoker)delegate{ Invalidate(); });
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if(Core?.Chain != null)
				Core.Chain.BlockAdded += OnBlockAdded;
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);

			if(Core?.Chain != null)
				Core.Chain.BlockAdded -= OnBlockAdded;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);

			if(!Mode)
			{
				e.Graphics.Clear(Color.White);

				if(Core?.Chain != null)
				{
					lock(Core.Lock)
					{
						var s = 8;
						var b = 2;
						var uset = true;
	
						var rounds = new List<Round>();
	
						int nmaxid = 0;
						int nmaxvoters = 0;
						int nmaxdate = 0;
	
						var f = "";

						do 
						{
							rounds.Clear();
	
							var n = Math.Min(Height/s-1, Core.Chain.LastNonEmptyRound.Id + 1);
							
							for(int i = Core.Chain.LastNonEmptyRound.Id - n + 1; i <= Core.Chain.LastNonEmptyRound.Id; i++)
							{
								var r = Core.Chain.FindRound(i);
								rounds.Add(r);
	
								if(uset)
								{
									nmaxid = Math.Max(nmaxid.ToString().Length, i.ToString().Length);
		
									if(r != null)
									{
										nmaxvoters = Math.Max(nmaxvoters, (r.Members != null ? r.Members.Count.ToString().Length : 0));
									}

									if(r != null)
									{
										nmaxdate = Math.Max(nmaxdate, r.Time.ToString().Length);
									}
								}
							}
		
							var members = rounds.Where(i => i != null).SelectMany(i => i.Blocks.Select(b => b.Member)).Distinct().OrderBy(i => i);

							f  = $"{{0,{nmaxid}}} {{1,{nmaxvoters}}} {{2}}{{3}} {{4,{nmaxdate}}}";

							if(rounds.Count() > 0)
							{
								var w = uset ? (int)e.Graphics.MeasureString(string.Format(f, 0, 0, 0, 0, 0, 0), Font).Width : 0;
			
								if(w + members.Count() * s < ClientSize.Width)
								{
									break;
								}
								else
								{
									s /= 2;
									b /= 2;
	
									if(s < 8)
										uset = false;
								}
							}
							else
								break;
						}
						while(s > 1);
	
						if(rounds.Count() > 0)
						{
							int y = 0;
	
							var members = rounds.Where(i => i != null).SelectMany(i => i.Blocks.Select(b => b.Member)).Distinct().OrderBy(i => i);
							
							foreach(var i in members)
							{
								if(!Brushes.ContainsKey(i))
								{
									Brushes[i] = new SolidBrush(Color.FromArgb(i[2], i[3], i[4]));
									Pens[i] = new Pen(Color.FromArgb(i[2], i[3], i[4]));
								}
							}
	
							foreach(var r in rounds)
							{
								if(r != null)
								{
									var x = 0;
									
									if(uset)
									{
										var t = string.Format(f, r.Id, r.Members != null ? r.Members.Count : 0, r.Voted ? "v" : " ", r.Confirmed ? "c" : " ", r.Time);
										
										x += (int)e.Graphics.MeasureString(t.Replace(' ', '_'), Font).Width;
										e.Graphics.DrawString(t, Font, System.Drawing.Brushes.Black, 0, y-1);
									}
			
									foreach(var m in members)
									{
										var block = r.Blocks.FirstOrDefault(i => i.Member == m);
		
										if(block != null)
										{
											if(block.Type == BlockType.Payload)		e.Graphics.FillRectangle(Brushes[m], x, y, s, s); else
											if(block.Type == BlockType.Vote)		e.Graphics.FillRectangle(Brushes[m], x+s/4, y+s/4, s/2, s/2); else
																					e.Graphics.DrawRectangle(Pens[m], x+1, y+1, s-3, s-3);
										}
			
										x += s;
									}
								}
								
								y += s;
							}
						}
					}
				}
			} 
			else
			{
				PaintEmission(e.Graphics);
			}
		}

		private void GraphControl_Resize(object sender, EventArgs e)
		{
			Invalidate();
			Update();
		}

		void PaintEmission(Graphics g)
		{ 
			g.Clear(Color.FromArgb(16,16,16));

			if(stat == null)
			{
				stat = new List<Coin[]>(100000);

				var rnd = new Random();
	
				emission = 0;
				spent = 0;
				
				Coin f = Emission.FactorStart;
				//Coin l = 1;
	
				do
				{
					var wei = Web3.Convert.ToWei((decimal)(rnd.NextDouble() * Math.Pow(10, rnd.Next(0, 4))));

					var r = Emission.Calculate(spent, f, wei);
					
					f			= r.Factor;
					emission	+= r.Amount;
					spent		+= wei;
						
					stat.Add(new Coin[] {f, emission, Coin.FromWei(spent)});
				}
				while(f < 1000);
			}

			var h = ClientSize.Height;
			var w = ClientSize.Width;

			for(var x=1f; x < w; x++)
			{
				g.DrawLine(System.Drawing.Pens.DarkMagenta,	x - 1,	(float)(h - h * stat[(int)((x -1) * stat.Count / w)][2]/stat.Last()[2]).ToDecimal(),  
															x,		(float)(h - h * stat[(int)(x	  * stat.Count / w)][2]/stat.Last()[2]).ToDecimal());

				g.DrawLine(System.Drawing.Pens.DarkCyan,	x - 1,	(float)(h - h * stat[(int)((x -1) * stat.Count / w)][1]/stat.Last()[1]).ToDecimal(),  
															x,		(float)(h - h * stat[(int)(x	  * stat.Count / w)][1]/stat.Last()[1]).ToDecimal());
			}

			g.DrawString("Emission Simulation", Font, System.Drawing.Brushes.Gray, 0, 0);
			
			var eth = $"Estimated Total ETH Spent:    {Web3.Convert.FromWei(spent), 30}";
			var unt = $"Estimated Total UNT Emitted : {emission.ToDecimal(), 30}";
			
			var eths = g.MeasureString(eth, Font);
			var unts = g.MeasureString(unt, Font);

			g.DrawString(eth, Font, System.Drawing.Brushes.DarkMagenta, w - eths.Width, h - eths.Height - unts.Height);
			g.DrawString(unt, Font, System.Drawing.Brushes.DarkCyan,	w - unts.Width, h - unts.Height);
		}
	}
}
