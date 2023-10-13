using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using RocksDbSharp;

namespace Uccs.Net
{
	[Flags]
	public enum Availability
	{
		None				= 0,
		Full				= 0b_______1,
		Minimal				= 0b______10,
		Partial				= 0b_____100,
		Complete			= 0b____1000, 
		CompletePartial		= 0b___10000, 
		Incremental			= 0b__100000, 
		IncrementalPartial	= 0b_1000000, 
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
		public bool				Completed => Length == -1; 
			
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
						 			
		public void Reset()
		{
			Length = -1;
			Pieces = new bool[Pieces.Length];
			Release.Save();
		}
						 			
		public void Complete()
		{
			Length = -1;
			Release.Save();
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
	}

	public class Release
	{
		public ResourceAddress					Address;
		public byte[]							Hash;
		public ResourceType						Type;
		public List<MembersResponse.Member>		DeclaredOn = new();
		public MembersResponse.Member[]			DeclareTo;
		public Availability						_Availability;
		List<ReleaseFile>						_Files;
		bool									Loaded;
		ResourceHub								Hub;
		public string							Path => System.IO.Path.Join(Hub.ResourcesPath, Address.Author, Hub.Escape(Address.Resource), Hash.ToHex());

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

		public Release(ResourceHub hub, ResourceAddress address, ResourceType type, byte[] hash)
		{
			Hub = hub;
			Address = address;
			Hash = hash;
			Type = type;
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

		public ReleaseFile AddFile(string path, byte[] data)
		{
			if(Files == null)
			{
				_Files = new();
				Loaded = true;
			}

			var f = new ReleaseFile(this) {Path = path};
			Files.Add(f);

			WriteFile(path, 0, data);

			f.Complete(); /// implicit Save called

			return f;
		}

		public void RemoveFile(ReleaseFile file)
		{
			Files.Remove(file);
		}

		public void Complete(Availability availability)
		{
			_Availability = availability;

			//if(availability == Availability.Full)
			//{
			//	_Files?.Clear();
			//}

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
	
					Type = (ResourceType)r.ReadByte();
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
								
				w.Write((byte)Type);
				w.Write((byte)Availability);
				w.Write(Files);

				b.Put(Hash, s.ToArray(), Hub.Family);
									
				Hub.Sun.Database.Write(b);
			}
		}

		public string AddressToPath(string file)
		{
			return System.IO.Path.Join(Path, file);
		}

		public byte[] ReadFile(string file, long offset, long length)
		{
			using(var s = new FileStream(AddressToPath(file), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, ResourceHub.PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public void WriteFile(string file, long offset, byte[] data)
		{
			var d = System.IO.Path.GetDirectoryName(AddressToPath(file));

			if(!Directory.Exists(d))
			{
				Directory.CreateDirectory(d);
			}

			using(var s = new FileStream(AddressToPath(file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public ReleaseFile Find(string filepath)
		{
			return Files?.Find(i => i.Path == filepath);
		}
		
		public bool IsReady(string filepath)
		{
			if(Availability == Availability.Full)
				return true;

			var f = Find(filepath);
			
			if(f == null)
				return false;

			return f.Completed;
		}

		public byte[] ReadFile(string file)
		{
			return File.ReadAllBytes(AddressToPath(file));
		}

		public byte[] Hashify(string path)
		{
			return Hub.Zone.Cryptography.HashFile(File.ReadAllBytes(AddressToPath(path)));
		}

		public long GetLength(string path)
		{
			return Find(path) != null ? new FileInfo(AddressToPath(path)).Length : -1;
		}

	}
}
