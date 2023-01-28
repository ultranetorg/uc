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

namespace UC.Sun.FUI
{
	public partial class ChainMonitor : UserControl
	{
		Dictionary<Account, Brush>	Brushes = new Dictionary<Account, Brush>();
		Dictionary<Account, Pen>	Pens = new Dictionary<Account, Pen>();
		bool						Mode = false;

		static List<Coin[]>			stat;
		Coin						emission = 0;
		BigInteger					spent = 0;

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

		public void OnBlockAdded(Block b)
		{
			BeginInvoke((MethodInvoker)delegate{ Invalidate(); });
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if(!Mode)
			{
				e.Graphics.Clear(Color.White);

				if(Core?.Database != null)
				{
					lock(Core.Lock)
					{
						if(!Core.Database.Tail.Any())
							return;

						var s = 8;
						var b = 2;
						var showt = true;
	
						var rounds = new List<Round>();
	
						int nid = 0;
						int nmemebers = 0;
						int njrs = 0;
						int nj = 0;
						int nl = 0;
						int ndate = 0;
	
						var f = "";

						do 
						{
							rounds.Clear();
	
							var n = Math.Min(Height/s-1, Core.Database.LastNonEmptyRound.Id + 1);
							
							for(int i = Core.Database.LastNonEmptyRound.Id - n + 1; i <= Core.Database.LastNonEmptyRound.Id; i++)
							{
								var r = Core.Database.FindRound(i);
								rounds.Add(r);
	
								if(showt && r != null)
								{
									nid = Math.Max(nid, i.ToString().Length);
									njrs = Math.Max(njrs, r.JoinRequests.Count().ToString().Length);
									nj = Math.Max(nj, r.ConfirmedJoiners.Count.ToString().Length);
									nl = Math.Max(nj, r.ConfirmedLeavers.Count.ToString().Length);
		
									if(r != null)
										nmemebers = Math.Max(nmemebers, r.Members.Count.ToString().Length);

									if(r != null)
										ndate = Math.Max(ndate, r.Time.ToString().Length);
								}
							}
		
							var members = rounds.Where(i => i != null).SelectMany(i => i.Blocks.Select(b => b.Generator)).Distinct().OrderBy(i => i);

							f  = $"{{0,{nid}}} {{1,{nmemebers}}} {{2,{njrs}}} {{3,{nj}}} {{4,{nl}}} {{5}}{{6}} {{7,{ndate}}}";

							if(rounds.Count() > 0)
							{
								var w = showt ? (int)e.Graphics.MeasureString(string.Format(f, 0, 0, 0, 0, 0, 0, 0, 0, 0), Font).Width : 0;
			
								if(w + members.Count() * s < ClientSize.Width)
								{
									break;
								}
								else
								{
									s /= 2;
									b /= 2;
	
									if(s < 8)
										showt = false;
								}
							}
							else
								break;
						}
						while(s > 1);
	
						if(rounds.Count() > 0)
						{
							int y = 0;
	
							var members = rounds.Where(i => i != null).SelectMany(i => i.Blocks.Select(b => b.Generator)).Distinct().OrderBy(i => i);
							
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
									
									if(showt)
									{
										var t = string.Format(	f, 
																r.Id, 
																r.Members.Count, 
																r.JoinRequests.Count(),
																r.ConfirmedJoiners.Count,
																r.ConfirmedLeavers.Count,
																r.Voted ? "v" : " ",
																r.Confirmed ? "c" : " ",
																r.Time);
										
										x += (int)e.Graphics.MeasureString(t, Font, int.MaxValue).Width;
										e.Graphics.DrawString(t, Font, System.Drawing.Brushes.Black, 0, y-1);
									}
			
									foreach(var m in members)
									{
										var block = r.Blocks.FirstOrDefault(i => i.Generator == m);
		
										if(block != null)
										{
											if(block.Type == BlockType.Payload)		e.Graphics.FillRectangle(Brushes[m], x, y, s, s); else
											if(block.Type == BlockType.Vote)		e.Graphics.DrawRectangle(Pens[m], x+1, y+1, s-3, s-3); else
																					e.Graphics.FillRectangle(Brushes[m], x+s/4, y+s/4, s/2, s/2);
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
