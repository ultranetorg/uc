﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class ZoneEntry : ITableEntry<Guid>, IBinarySerializable
	{
		public EntityId		Id { get; set; }
		public Guid			Key => Address;
		
		public Guid			Address { get; set; }
		public Money		Balance { get; set; }
		public byte[]		IncomingBlockEmptyHash { get; set; }
		public byte[]		IncomingBlockHash { get; set; }
		public byte[]		OutgoingBlockEmptyHash { get; set; }
		public byte[]		OutgoingBlockHash { get; set; }
		
		Mcv					Mcv;

		public ZoneEntry()
		{
		}

		public ZoneEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public ZoneEntry Clone()
		{
			return new ZoneEntry(Mcv){	Id = Id,
										Address = Address,
										Balance = Balance};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Address);
			writer.Write(Balance);
		}

		public void Read(BinaryReader reader)
		{
			Address = reader.ReadGuid();
			Balance = reader.Read<Money>();
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);

		}

		public void ReadMain(BinaryReader r)
		{
			Read(r);
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}
	}
}