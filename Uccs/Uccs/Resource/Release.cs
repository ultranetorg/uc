using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nethereum.Hex.HexConvertors.Extensions;
using RocksDbSharp;

namespace Uccs.Net
{
	[Flags]
	public enum Availability
	{
		Null				= 0,
		IncrementalFull		= 0b00000001, 
		IncrementalPartial	= 0b00000010, 
		CompleteFull		= 0b00000100, 
		CompletePartial		= 0b00001000, 
		Full				= 0b00010000, 
		Minimal				= 0b00100000, 
	}

	public class ReleaseFile : IBinarySerializable
	{
		public string	Path;
		public int		PieceLength;
		public long		Length;
		public bool[]	Pieces;
		Release			Release;

		public IEnumerable<int>	CompletedPieces => Pieces.Select((e, i) => e ? i : -1).Where(i => i != -1);
		public long				CompletedLength => CompletedPieces.Count() * PieceLength - (Pieces.Last() ? PieceLength - Length % PieceLength : 0); /// take the tail into account
		public bool				Completed => PieceLength == -1; 
			
		public ReleaseFile(Release release)
		{
			Release = release;
		}

		public ReleaseFile(Release release, string path, long length, int piecelength, int piececount)
		{
			Release = release;
			Path = path;
			Length = length;
			PieceLength = piecelength;
			Pieces = new bool[piececount];
		}
		
		public override string ToString()
		{
			return $"{Path}, Length+{Length}, PieceLength={PieceLength} Pieces={{{Pieces?.Length}}}";
		}

		public void CompletePiece(int i)
		{
			Pieces[i] = true;
			Release.Save();
		}

		public void Read(BinaryReader reader)
		{
			Path = reader.ReadUtf8();
			Length = reader.ReadInt64();
										
			if(Length > 0)
			{
				PieceLength = reader.ReadInt32();
				Pieces = reader.ReadArray(() => reader.ReadBoolean());
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.WriteUtf8(Path);
			writer.Write(Length);
				
			if(Length > 0)
			{
				writer.Write(PieceLength);
				writer.Write(Pieces, i => writer.Write(i));
			}
		}
						 			
		public void Complete()
		{
			Length = -1;
			Release.Save();
		}
	}

	public class Release
	{
		public ResourceAddress					Address;
		public byte[]							Hash;
		public List<MembersResponse.Member>		DeclaredOn = new();
		public MembersResponse.Member[]			DeclareTo;
		public Availability						_Availability;
		List<ReleaseFile>						_Files;
		bool									Loaded;
		ResourceHub								Hub;

		public List<ReleaseFile> Files
		{
			get
			{ 
				Load();
				return _Files; 
			}
		}

		public Availability Availability
		{
			get
			{ 
				Load();
				return _Availability; 
			}
		}

		public Release(ResourceHub hub, ResourceAddress address, byte[] hash)
		{
			Hub = hub;
			Address = address;
			Hash = hash;
			//Type = type;
		}

		public override string ToString()
		{
			return $"{Address}, {Hash.ToHex()}, Availability={_Availability}, Files={{{_Files?.Count}}}";
		}

		public ReleaseFile AddFile(string path, long length, int piecelength, int piececount)
		{
			if(Files == null)
			{
				_Files = new();
				Loaded = true;
			}

			Files.Add(new ReleaseFile(this, path, length, piecelength, piececount));

			Save();

			return Files.Last();
		}

		public void Complete(Availability availability)
		{
			_Availability = availability;
			_Files.Clear();
			Save();
		}

		void Load()
		{
			if(!Loaded)
			{
				var d = Hub.Sun.Database.Get(Hash, Hub.Family);
										
				if(d != null)
				{
					var s = new MemoryStream(d);
					var r = new BinaryReader(s);
	
					_Availability = (Availability)r.ReadByte();
					_Files = r.Read(() => new ReleaseFile(this), f => f.Read(r)).ToList();
				}
			}

			Loaded = true;
		}

		internal void Save()
		{
			using(var b = new WriteBatch())
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				w.Write((byte)Availability);
				w.Write(Files);

				b.Put(Hash, s.ToArray(), Hub.Family);
									
				Hub.Sun.Database.Write(b);
			}
		}
	}
}
