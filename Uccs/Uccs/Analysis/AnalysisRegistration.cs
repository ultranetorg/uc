using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public class AnalysisRegistration : Operation
	{
		public ReleaseAddress	Release { get; set; }
		public Money			Fee { get; set; }

		public override string	Description => $"Release={Release}, Fee={Fee}";
		public override bool	Valid => Fee > 0;

		public AnalysisRegistration()
		{
		}
		
		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Fee);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Release = reader.Read(ReleaseAddress.FromType);
			Fee		= reader.Read<Money>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(round.Analyzers.Count == 0)
			{
				Error = NoAnalyzers;
				return;
			}

			var z = mcv.Releases.Find(Release, round.Id);

			if(z != null && z.Flags.HasFlag(ReleaseFlag.Analysis) && round.ConsensusTime.Days - z.StartedAt.Days < 2) /// 1..2 days
			{
				Error = AlreadyExists;
				return;
			}

			z = Affect(round, Release);

			if(z.Expiration - round.ConsensusTime < Time.FromYears(1))
			{
				z.Expiration = round.ConsensusTime + Time.FromYears(1);
				PayForEntity(round, 1);
			}

			z.Flags		|= ReleaseFlag.Analysis;
			z.Fee		= Fee;
			z.StartedAt = round.ConsensusTime;
			z.Consil	= (byte)round.Analyzers.Count;

			var s = Affect(round, Signer);
			s.Balance -= Fee;
		}
	}
}
