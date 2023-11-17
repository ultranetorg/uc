using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public class AnalysisOrder : Operation
	{
		public byte[]			Release { get; set; }
		public Money			Fee { get; set; }

		public override string	Description => $"Release={Release.ToHex()}, Fee={Fee}";
		public override bool	Valid => Fee > 0;

		public AnalysisOrder()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Fee);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Release = reader.ReadHash();
			Fee		= reader.ReadMoney();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var a = mcv.Analyses.Find(Release, round.Id);

			if(a != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = Affect(round, Release);
			a.Fee = Fee;
			a.StartedAt = round.Id;
			a.Consil = (byte)round.Analyzers.Count;

			var s = Affect(round, Signer);
			s.Balance -= Fee;
		}
	}
}
