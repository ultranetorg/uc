using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	public class LocalFile : IBinarySerializable
	{
		public string			Path { get; set; }
		public int				PieceLength { get; protected set; }
		public long				Length { get; protected set; }
		public bool[]			Pieces;
		LocalRelease			Release;

		public IEnumerable<int>	CompletedPieces => Pieces.Select((e, i) => e ? i : -1).Where(i => i != -1);
		public long				CompletedLength => CompletedPieces.Count() * PieceLength - (Pieces.Last() ? PieceLength - Length % PieceLength : 0); /// take the tail into account
		public bool				Completed => Length == -1; 
			
		public LocalFile()
		{
		}

		public LocalFile(LocalRelease release)
		{
			Release = release;
		}

		public LocalFile(LocalRelease release, string path, long length, int piecelength, int piececount)
		{
			Release = release;
			Path = path;
			Length = length;
			PieceLength = piecelength;
			Pieces = new bool[piececount];
		}
		
		public override string ToString()
		{
			return $"{Path}, Length={Length}, PieceLength={PieceLength}, Pieces={{{Pieces?.Length}}}";
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

	public class LocalRelease
	{
		public byte[]							Hash;
		public ResourceType						Type;
		public List<MembersResponse.Member>		DeclaredOn = new();
		public MembersResponse.Member[]			DeclareTo;
		public Availability						_Availability;
		List<LocalFile>							_Files;
		bool									Loaded;
		ResourceHub								Hub;
		public string							Path => System.IO.Path.Join(Hub.ReleasesPath, Hash.ToHex());

		public List<LocalFile> Files
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

		public LocalRelease(ResourceHub hub, byte[] hash, ResourceType type)	
		{
			Hub = hub;
			Hash = hash;
			Type = type;
		}

		public override string ToString()
		{
			return $"{Hash.ToHex()}, Availability={_Availability}, Files={{{_Files?.Count}}}";
		}

		public LocalFile AddFile(string path, long length, int piecelength, int piececount)
		{
			Load();
	
			if(Files == null)
			{
				_Files = new();
				Loaded = true;
			}

			Files.Add(new LocalFile(this, path, length, piecelength, piececount));

			Save();

			return Files.Last();
		}

		public LocalFile AddFile(string path, byte[] data)
		{
			Load();
		
			if(Files == null)
			{
				_Files = new();
				Loaded = true;
			}

			var f = new LocalFile(this) {Path = path};
			Files.Add(f);

			WriteFile(path, 0, data);

			f.Complete(); /// implicit Save called

			return f;
		}

		public void RemoveFile(LocalFile file)
		{
			Load();
		
			Files.Remove(file);
		}

		public void Complete(Availability availability)
		{
			Load();
		
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
				var d = Hub.Sun.Database.Get(Hash, Hub.ReleaseFamily);
										
				if(d != null)
				{
					var s = new MemoryStream(d);
					var r = new BinaryReader(s);
	
					Type = (ResourceType)r.ReadByte();
					_Availability = (Availability)r.ReadByte();
					_Files = r.Read(() => new LocalFile(this), f => f.Read(r)).ToList();
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

				b.Put(Hash, s.ToArray(), Hub.ReleaseFamily);
									
				Hub.Sun.Database.Write(b);
			}
		}

		public string MapPath(string file)
		{
			return System.IO.Path.Join(Path, file);
		}

		public byte[] ReadFile(string file, long offset, long length)
		{
			using(var s = new FileStream(MapPath(file), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, ResourceHub.PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public void WriteFile(string file, long offset, byte[] data)
		{
			var d = System.IO.Path.GetDirectoryName(MapPath(file));

			if(!Directory.Exists(d))
			{
				Directory.CreateDirectory(d);
			}

			using(var s = new FileStream(MapPath(file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public LocalFile Find(string filepath)
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
			return File.ReadAllBytes(MapPath(file));
		}

		public byte[] Hashify(string path)
		{
			return Hub.Zone.Cryptography.HashFile(File.ReadAllBytes(MapPath(path)));
		}

		public long GetLength(string path)
		{
			return Find(path) != null ? new FileInfo(MapPath(path)).Length : -1;
		}

	}
}
