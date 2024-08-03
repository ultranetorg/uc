using RocksDbSharp;

namespace Uccs.Rdn
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

	public enum LocalFileStatus
	{
		None, Inited, Completed
	}

	public class LocalFile : IBinarySerializable
	{
		public string			Path;
		public string			LocalPath;
		public int				PieceLength { get; protected set; } = -1;
		public long				Length { get; protected set; }
		public bool[]			Pieces;
		public object			Activity;
		LocalRelease			Release;
		public byte[]			Data;

		public IEnumerable<int>	CompletedPieces => Pieces.Select((e, i) => e ? i : -1).Where(i => i != -1);
		public long				CompletedLength => CompletedPieces.Count() * PieceLength - (Pieces.Last() ? PieceLength - Length % PieceLength : 0); /// take the tail into account
		public LocalFileStatus	Status; 
			
		public LocalFile(LocalRelease release)
		{
			Release = release;
		}
			
		public LocalFile(LocalRelease release, string path, string localpath, byte[] data)
		{
			Release		= release;
			Path		= path;
			LocalPath	= localpath;
			Data		= data;
			Length		= data != null ? data.Length : (File.Exists(localpath) ? new FileInfo(localpath).Length : 0);
		}
				
		public override string ToString()
		{
			return $"{Path}, Length={Length}, PieceLength={PieceLength}, Pieces={{{Pieces?.Length}}}";
		}

		public void Init(long length, int piecelength, int piececount)
		{
			Status = LocalFileStatus.Inited;
			Length = length;

			if(length > 0)
			{
				PieceLength = piecelength;
				Pieces	= new bool[piececount];
			}

			if(Path.StartsWith('\0'))
				Data = new byte[length];

			Release.Save();
		}
						 			
 		public void Reset()
 		{
			Status = LocalFileStatus.None;
			Release.Save();
 		}
						 			
		public void Complete()
		{
			Status = LocalFileStatus.Completed;
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
			Status = (LocalFileStatus)reader.ReadByte();
			Length = reader.ReadInt64();

			if(Path.StartsWith('\0'))
				Data = reader.ReadBytes();
			else
				LocalPath = reader.ReadUtf8();
													
			if(Status == LocalFileStatus.Inited)
			{
				PieceLength = reader.ReadInt32();
				Pieces = reader.ReadArray(() => reader.ReadBoolean());
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.WriteUtf8(Path);
			writer.Write((byte)Status);
			writer.Write(Length);

			if(Path.StartsWith('\0'))
				writer.WriteBytes(Data);
			else
				writer.WriteUtf8(LocalPath);
				
			if(Status == LocalFileStatus.Inited)
			{
				writer.Write(PieceLength);
				writer.Write(Pieces, i => writer.Write(i));
			}
		}
		
		public void Write(long offset, byte[] data)
		{
			if(LocalPath != null)
			{
				var d = System.IO.Path.GetDirectoryName(LocalPath);
			
				if(!Directory.Exists(d))
				{
					Directory.CreateDirectory(d);
				}
			}
		
			using(Stream s = LocalPath == null ? new MemoryStream(Data) : new FileStream(LocalPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public byte[] Read(long offset = 0, long length = -1)
		{
			using(Stream s = LocalPath == null ? new MemoryStream(Data) : new FileStream(LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[length == -1 ? Length : length];
	
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
		public Member					Member;
		public DeclarationStatus		Status;
		public DateTime					Failed;
	}

	public class LocalRelease
	{
		public const string				Index = "\0index";

		public Urr						Address;
		public List<Declaration>		DeclaredOn = new();
		//public string					Path => System.IO.Path.Join(Hub.ReleasesPath, ResourceHub.Escape(Address.ToString()));
		public object					Activity;
		Availability					_Availability;
		//long							_Type;
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

		//public long Type
		//{
		//	get
		//	{ 
		//		Load();
		//		return _Type; 
		//	}
		//}

		public LocalRelease(ResourceHub hub, Urr address)	
		{
			Hub = hub;
			Address = address;
			//_Type = type;
		}

		public override string ToString()
		{
			return $"{Address}, Availability={Availability}, Files={{{Files?.Count}}}";
		}

		public LocalFile AddEmpty(string path, string localpath)
		{
			if(Files.Any(i => i.Path == path))
				throw new IntegrityException($"File {path} already exists");

			Files.Add(new LocalFile(this, path, localpath, null));

			Save();

			return Files.Last();
		}

		//public LocalFile AddExisting(string path, string localpath)
		//{
		//	if(Files.Any(i => i.Path == path))
		//		throw new IntegrityException($"File {path} already exists");
		//
		//	var f = new LocalFile(this, path, localpath, null);
		//	Files.Add(f);
		//
		//	f.Complete(); /// implicit Save called
		//
		//	return f;
		//}

		public LocalFile AddCompleted(string path, string localpath, byte[] data)
		{
			if(Files.Any(i => i.Path == path))
				throw new IntegrityException($"File {path} already exists");

			var f = new LocalFile(this, path, path.StartsWith('\0') ? null : (localpath ?? Hub.ToReleases(Address)), data);
			Files.Add(f);

			if(!path.StartsWith('\0') && data != null)
			{
				f.Write(0, data);
			}

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
				var d = Hub.Node.Database.Get(Address.Raw, Hub.ReleaseFamily);
										
				if(d != null)
				{
					var s = new MemoryStream(d);
					var r = new BinaryReader(s);
	
					//_Type			= r.Read7BitEncodedInt64();
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
								
				//w.Write7BitEncodedInt64(Type);
				w.Write((byte)Availability);
				w.Write(Files);

				b.Put(Address.Raw, s.ToArray(), Hub.ReleaseFamily);
									
				Hub.Node.Database.Write(b);
			}
		}

 		//public string MapPath(string file)
 		//{
 		//	return System.IO.Path.Join(Path, file);
 		//}

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

			return f.Status == LocalFileStatus.Completed;
		}

		public byte[] Hashify(string path)
		{
			return Hub.Zone.Cryptography.HashFile(Find(path).Read());
		}
	}
}
