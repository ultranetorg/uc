using RocksDbSharp;

namespace Uccs.Rdn;

[Flags]
public enum Availability : byte
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

public enum ReleaseFileStatus
{
	None, Inited, Completed
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

public class ReleaseFile : IBinarySerializable
{
	public string				Path;
	public byte[]				Data;
	public string				DataPath;
	public int					PieceLength { get; protected set; } = -1;
	public long					Length { get; protected set; }
	public bool[]				Pieces;
	public object				Activity;
	Release						Release;

	public bool					IsMeta => Path.StartsWith('\0');

	public IEnumerable<int>		CompletedPieces => Pieces.Select((e, i) => e ? i : -1).Where(i => i != -1);
	public long					CompletedLength => CompletedPieces.Count() * PieceLength - (Pieces.Last() ? PieceLength - Length % PieceLength : 0); /// take the tail into account
	public ReleaseFileStatus	Status; 
		
	public ReleaseFile(Release release)
	{
		Release = release;
	}
		
	public ReleaseFile(Release release, string path, string datapath, byte[] data)
	{
		Release		= release;
		Path		= path;
		DataPath	= datapath;
		Data		= data;
		Length		= data != null ? data.Length : (File.Exists(datapath) ? new FileInfo(datapath).Length : 0);
	}
			
	public override string ToString()
	{
		return $"{Path}, Length={Length}, PieceLength={PieceLength}, Pieces={{{Pieces?.Length}}}";
	}

	public void Init(long length, int piecelength, int piececount)
	{
		Status = ReleaseFileStatus.Inited;
		Length = length;

		if(length > 0)
		{
			PieceLength = piecelength;
			Pieces	= new bool[piececount];
		}

		if(IsMeta)
			Data = new byte[length];

		Release.Save();
	}
					 			
 	public void Reset()
 	{
		Status = ReleaseFileStatus.None;
		Release.Save();
 	}
					 			
	public void Complete()
	{
		Status = ReleaseFileStatus.Completed;
		Release.Save();
	}

	public void CompletePiece(int i)
	{
		Pieces[i] = true;
		Release.Save();
	}

	public void Read(Reader reader)
	{
		Path = reader.ReadUtf8();
		Status = (ReleaseFileStatus)reader.ReadByte();
		Length = reader.ReadInt64();

		if(IsMeta)
			Data = reader.ReadBytes();
		else
			DataPath = reader.ReadUtf8();
												
		if(Status == ReleaseFileStatus.Inited)
		{
			PieceLength = reader.ReadInt32();
			Pieces = reader.ReadArray(() => reader.ReadBoolean());
		}
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Path);
		writer.Write((byte)Status);
		writer.Write(Length);

		if(IsMeta)
			writer.WriteBytes(Data);
		else
			writer.WriteUtf8(DataPath);
			
		if(Status == ReleaseFileStatus.Inited)
		{
			writer.Write(PieceLength);
			writer.Write(Pieces, i => writer.Write(i));
		}
	}
	
	public void Write(long offset, byte[] data)
	{
		if(DataPath != null)
		{
			var d = System.IO.Path.GetDirectoryName(DataPath);
		
			if(!Directory.Exists(d))
			{
				Directory.CreateDirectory(d);
			}
		}
	
		using(Stream s = DataPath == null ? new MemoryStream(Data) : new FileStream(DataPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
		{
			s.Seek(offset, SeekOrigin.Begin);
			s.Write(data);
		}
	}

	public byte[] Read(long offset = 0, long length = -1)
	{
		using(Stream s = DataPath == null ? new MemoryStream(Data) : new FileStream(DataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			s.Seek(offset, SeekOrigin.Begin);
			
			var b = new byte[length == -1 ? Length : length];

			s.Read(b);

			return b;
		}
	}
}

public class Release
{
	public const string				Index = "\0index";

	public AutoId					Resource;
	public Urn						Address;
	public List<Declaration>		DeclaredOn = new();
	//public string					Path => System.IO.Path.Join(Hub.ReleasesPath, ResourceHub.Escape(Address.ToString()));
	public object					Activity;
	Availability					_Availability;
	//long							_Type;
	List<ReleaseFile>				_Files;
	bool							Loaded;
	ResourceHub						Hub;

	public System.Diagnostics.StackTrace		__StackTrace;

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

	public Release(ResourceHub hub, Urn address)	
	{
		Hub = hub;
		Address = address;
		//_Type = type;
	}

	public Release(ResourceHub hub)
	{
		Hub = hub;
	}

	public Release(ResourceHub hub, Dictionary<object, string> files)
	{
		Hub = hub;

		_Files = [];
		Loaded = true;

		var index = new Xon(new XonBinaryValueSerializator());

		foreach(var i in files)
		{	
			var path = i.Value.Replace(Path.DirectorySeparatorChar, '/');
			index.Add(path).Value = Hub.Net.Cryptography.HashFile(i.Key is byte[] b ? b : File.ReadAllBytes(i.Key as string));
		}

		var ms = new MemoryStream();
		index.Save(new XonBinaryWriter(ms));

		Address = new Hcid(Hub.Net.Cryptography.HashFile(ms.ToArray()));

		foreach(var i in files)
		{
			AddCompleted(i.Value, i.Key as string, i.Key as byte[]);
		}

		AddCompleted(Release.Index, null, ms.ToArray());
	}

	public override string ToString()
	{
		return $"{Address}, Availability={Availability}, Files={{{Files?.Count}}}";
	}

	public Xon LoadIndex()
	{
		var index = new Xon(Find(Index).Read());
		return index;
	}

	public ReleaseFile AddEmpty(string path, string localpath)
	{
		if(Files.Any(i => i.Path == path))
			throw new IntegrityException($"File {path} already exists");

		Files.Add(new ReleaseFile(this, path, localpath, null));

		Save();

		return Files.Last();
	}

	public ReleaseFile AddCompleted(string path, string datapath, byte[] data)
	{
		if(Files.Any(i => i.Path == path))
			throw new IntegrityException($"File {path} already exists");

		var f = new ReleaseFile(this, path, path.StartsWith('\0') ? null : (datapath ?? System.IO.Path.Join(Hub.ToReleases(Address), path)), data);
		Files.Add(f);

		if(!f.IsMeta && data != null)
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
		if(Address == null)
			return;

		if(!Loaded)
		{
			var d = Hub.Node.Database.Get(Address.Raw, Hub.ReleaseFamily);
									
			if(d != null)
			{
				using var r = new Reader(d);

				Resource		= r.Read<AutoId>();
				_Availability	= r.Read<Availability>();
				_Files			= r.Read(() => new ReleaseFile(this), f => f.Read(r)).ToList();
			}
			else
			{
				_Files = [];
			}
		}

		Loaded = true;
	}

	internal void Save()
	{
		if(Address == null)
			return;

		using(var b = new WriteBatch())
		{
			using var s = new MemoryStream();
			var w = new Writer(s);
								
			w.WriteNullable(Resource);
			w.Write(Availability);
			w.Write(Files);
	
			b.Put(Address.Raw, s.ToArray(), Hub.ReleaseFamily);
									
			Hub.Node.Database.Write(b);
		}
	}

	public ReleaseFile Find(string filepath)
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

		return f.Status == ReleaseFileStatus.Completed;
	}

	public byte[] Hashify(string path)
	{
		return Hub.Net.Cryptography.HashFile(Find(path).Read());
	}
}
