using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
/*
	public class Consensus : IEquatable<Consensus>, IBinarySerializable
	{
		public const byte		PrefixLength = 4;
		public static readonly	Consensus Empty = new Consensus {	Parent = Cryptography.ZeroHash,
																	Payloads = new(), 
																	GeneratorJoiners = new(), 
																	GeneratorLeavers = new(), 
																	HubJoiners = new(), 
																	HubLeavers = new(), 
																	AnalyzerJoiners = new(), 
																	AnalyzerLeavers = new(), 
																	FundJoiners = new(), 
																	FundLeavers = new(),
																	Violators = new() };
		public byte[]			Parent;
		public ChainTime		Time;
		public List<byte[]>		Payloads;
		public List<byte[]>		GeneratorJoiners;
		public List<byte[]>		GeneratorLeavers;
		public List<byte[]>		HubJoiners;
		public List<byte[]>		HubLeavers;
		public List<byte[]>		AnalyzerJoiners;
		public List<byte[]>		AnalyzerLeavers;
		public List<byte[]>		FundJoiners;
		public List<byte[]>		FundLeavers;
		public List<byte[]>		Violators;

		public void WriteHashable(BinaryWriter w)
		{
			Write(w);
		}

		public void Write(BinaryWriter w)
		{
			#if DEBUG
			if(Payloads.Any(i => i.Length != PrefixLength))			throw new IntegrityException("Wrong Payloads Prefix length");
			if(Violators.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong Violators Prefix length");
			if(GeneratorJoiners.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong GeneratorJoiners Prefix length");
			if(GeneratorLeavers.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong GeneratorLeavers Prefix length");
			if(HubJoiners.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong HubJoiners Prefix length");
			if(HubLeavers.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong HubLeavers Prefix length");
			if(AnalyzerJoiners.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong AnalyzerJoiners Prefix length");
			if(AnalyzerLeavers.Any(i => i.Length != PrefixLength))	throw new IntegrityException("Wrong AnalyzerLeavers Prefix length");
			if(FundJoiners.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong FundJoiners Prefix length");
			if(FundLeavers.Any(i => i.Length != PrefixLength))		throw new IntegrityException("Wrong FundLeavers Prefix length");
			#endif

			w.Write(Parent);
			w.Write(Time);
			w.Write(Payloads,			i => w.Write(i));
			w.Write(Violators,			i => w.Write(i));
			w.Write(GeneratorJoiners,	i => w.Write(i));
			w.Write(GeneratorLeavers,	i => w.Write(i));
			w.Write(HubJoiners,			i => w.Write(i));
			w.Write(HubLeavers,			i => w.Write(i));
			w.Write(AnalyzerJoiners,	i => w.Write(i));
			w.Write(AnalyzerLeavers,	i => w.Write(i));
			w.Write(FundJoiners,		i => w.Write(i));
			w.Write(FundLeavers,		i => w.Write(i));
		}

		public void Read(BinaryReader r)
		{
			Parent				= r.ReadSha3();
			Time				= r.ReadTime();
			Payloads			= r.ReadList(() => r.ReadBytes(PrefixLength));
			Violators			= r.ReadList(() => r.ReadBytes(PrefixLength));
			GeneratorJoiners	= r.ReadList(() => r.ReadBytes(PrefixLength));
			GeneratorLeavers	= r.ReadList(() => r.ReadBytes(PrefixLength));
			HubJoiners			= r.ReadList(() => r.ReadBytes(PrefixLength));
			HubLeavers			= r.ReadList(() => r.ReadBytes(PrefixLength));
			AnalyzerJoiners		= r.ReadList(() => r.ReadBytes(PrefixLength));
			AnalyzerLeavers		= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundJoiners			= r.ReadList(() => r.ReadBytes(PrefixLength));
			FundLeavers			= r.ReadList(() => r.ReadBytes(PrefixLength));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Consensus);
		}

		public override int GetHashCode()
		{
			return	Parent[0] ^
					Time.Ticks.GetHashCode() ^
					Payloads.Count.GetHashCode() ^ 
					Violators.Count.GetHashCode() ^ 
					GeneratorJoiners.Count.GetHashCode() ^ 
					GeneratorLeavers.Count.GetHashCode() ^ 
					HubJoiners.Count.GetHashCode() ^ 
					HubLeavers.Count.GetHashCode() ^ 
					AnalyzerJoiners.Count.GetHashCode() ^ 
					AnalyzerLeavers.Count.GetHashCode() ^ 
					FundJoiners.Count.GetHashCode() ^ 
					FundLeavers.Count.GetHashCode();
		}

		public bool Equals(Consensus o)
		{
			return	Parent.SequenceEqual(o.Parent) &&
					Time == o.Time &&
					Payloads		.SequenceEqual(o.Payloads,			new BytesEqualityComparer()) &&
					Violators		.SequenceEqual(o.Violators,			new BytesEqualityComparer()) &&
					GeneratorJoiners.SequenceEqual(o.GeneratorJoiners,	new BytesEqualityComparer()) &&
					GeneratorLeavers.SequenceEqual(o.GeneratorLeavers,	new BytesEqualityComparer()) &&
					HubJoiners		.SequenceEqual(o.HubJoiners,		new BytesEqualityComparer()) &&
					HubLeavers		.SequenceEqual(o.HubLeavers,		new BytesEqualityComparer()) &&
					AnalyzerJoiners	.SequenceEqual(o.AnalyzerJoiners,	new BytesEqualityComparer()) &&
					AnalyzerLeavers	.SequenceEqual(o.AnalyzerLeavers,	new BytesEqualityComparer()) &&
					FundJoiners		.SequenceEqual(o.FundJoiners,		new BytesEqualityComparer()) &&
					FundLeavers		.SequenceEqual(o.FundLeavers,		new BytesEqualityComparer());
		}
	}*/
}
