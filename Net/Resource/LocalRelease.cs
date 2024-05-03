using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
		public string			LocalPath => Release.MapPath(Path);
		public int				PieceLength { get; protected set; } = -1;
		public long				Length { get; protected set; } = -1;
		public bool[]			Pieces;
		public object			Activity;
		LocalRelease			Release;

		public IEnumerable<int>	CompletedPieces => Pieces.Select((e, i) => e ? i : -1).Where(i => i != -1);
		public long				CompletedLength => CompletedPieces.Count() * PieceLength - (Pieces.Last() ? PieceLength - Length % PieceLength : 0); /// take the tail into account
		public bool				Completed => Length == -2; 
		public bool				Initialized => Length >= 0; 
			
		public LocalFile(LocalRelease release)
		{
			Release = release;
		}
			
		public LocalFile(LocalRelease release, string path)
		{
			Release = release;
			Path = path;
		}
		
		public override string ToString()
		{
			return $"{Path}, Length={Length}, PieceLength={PieceLength}, Pieces={{{Pieces?.Length}}}";
		}

		public void Init(long length, int piecelength, int piececount)
		{
			Length = length;

			if(length > 0)
			{
				PieceLength = piecelength;
				Pieces		= new bool[piececount];
			}

			Release.Save();
		}
						 			
 		public void Reset()
 		{
 			Length = -1;
			PieceLength = -1;
 			Pieces = null;

			Release.Save();
 		}
						 			
		public void Complete()
		{
			Length = -2;
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
		
		public void Write(long offset, byte[] data)
		{
			var d = System.IO.Path.GetDirectoryName(LocalPath);
		
			if(!Directory.Exists(d))
			{
				Directory.CreateDirectory(d);
			}
		
			using(var s = new FileStream(LocalPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public byte[] Read(long offset = 0, long length = -1)
		{
			using(var s = new FileStream(LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[length == -1 ? new FileInfo(LocalPath).Length : length];
	
				s.Read(b);
	
				return b;
			}
		}
	}

	public enum DeclarationStatus
	{
		None, InProgress, Accepted, Failed
	}

	public class Declaration
	{
		public MembersResponse.Member	Member;
		public DeclarationStatus		Status;
		public DateTime					Failed;
	}

	public class LocalRelease
	{
		public Urr						Address;
		public List<Declaration>		DeclaredOn = new();
		public string					Path => System.IO.Path.Join(Hub.ReleasesPath, ResourceHub.Escape(Address.ToString()));
		public object					Activity;
		Availability					_Availability;
		DataType						_Type;
		List<LocalFile>					_Files;
		bool							Loaded;
		ResourceHub						Hub;

		public System.Diagnostics.StackTrace		__StackTrace;

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

		public DataType Type
		{
			get
			{ 
				Load();
				return _Type; 
			}
		}

		public LocalRelease(ResourceHub hub, Urr address, DataType type)	
		{
			Hub = hub;
			Address = address;
			_Type = type;
		}

		public override string ToString()
		{
			return $"{Address}, Availability={Availability}, Files={{{Files?.Count}}}";
		}

		public LocalFile AddEmpty(string path)
		{
			if(Files.Any(i => i.Path == path))
				throw new IntegrityException($"File {path} already exists");

			Files.Add(new LocalFile(this, path));

			Save();

			return Files.Last();
		}

		public LocalFile AddCompleted(string path, byte[] data)
		{
			if(Files.Any(i => i.Path == path))
				throw new IntegrityException($"File {path} already exists");

			var f = new LocalFile(this, path);
			Files.Add(f);

			f.Write(0, data);

			f.Complete(); /// implicit Save called

			return f;
		}

		public void Complete(Availability availability)
		{
			Load();
		
			_Availability = availability;

			Save();
		}

		void Load()
		{
			if(!Loaded)
			{
				var d = Hub.Sun.Database.Get(Address.Raw, Hub.ReleaseFamily);
										
				if(d != null)
				{
					var s = new MemoryStream(d);
					var r = new BinaryReader(s);
	
					_Type			= (DataType)r.ReadByte();
					_Availability	= (Availability)r.ReadByte();
					_Files			= r.Read(() => new LocalFile(this), f => f.Read(r)).ToList();
				}
				else
				{
					_Files = new();
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

				b.Put(Address.Raw, s.ToArray(), Hub.ReleaseFamily);
									
				Hub.Sun.Database.Write(b);
			}
		}

		public string MapPath(string file)
		{
			return System.IO.Path.Join(Path, file);
		}

		//public byte[] ReadFile(string file, long offset, long length)
		//{
		//	using(var s = new FileStream(MapPath(file), FileMode.Open, FileAccess.Read, FileShare.Read))
		//	{
		//		s.Seek(offset, SeekOrigin.Begin);
		//		
		//		var b = new byte[Math.Min(length, ResourceHub.PieceMaxLength)];
		//
		//		s.Read(b);
		//
		//		return b;
		//	}
		//}
		//
		//public void WriteFile(string file, long offset, byte[] data)
		//{
		//	var d = System.IO.Path.GetDirectoryName(MapPath(file));
		//
		//	if(!Directory.Exists(d))
		//	{
		//		Directory.CreateDirectory(d);
		//	}
		//
		//	using(var s = new FileStream(MapPath(file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
		//	{
		//		s.Seek(offset, SeekOrigin.Begin);
		//		s.Write(data);
		//	}
		//}

		public LocalFile Find(string filepath)
		{
			return Files.Find(i => i.Path == filepath);
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
