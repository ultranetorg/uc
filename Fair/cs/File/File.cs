using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Uccs.Fair;

public enum FairMime : byte
{
	None, ImageJpg, ImagePng
}

public class File : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public EntityAddress	Owner { get; set; }
	public FairMime			Mime { get; set; }
    public int				Refs { get; set; }
	public byte[]			Data { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public File()
	{
	}

	public File(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return	new File(Mcv)
				{
					Id		= Id,
					Owner	= Owner,
					Mime	= Mime,
					Refs	= Refs,
					Data	= Data,
				};
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Owner	= reader.Read<EntityAddress>();
		Mime	= reader.Read<FairMime>();
		Refs	= reader.Read7BitEncodedInt();
		Data	= reader.ReadBytes();
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Owner);
		writer.Write(Mime);
		writer.Write7BitEncodedInt(Refs);
		writer.WriteBytes(Data);
	}

	public static FairMime GetImageFormat(byte[] data)
	{
		var image = Image.Identify(data);
			
		if(image == null)
			return FairMime.None;
        
		var format = image.Metadata.DecodedImageFormat;

		if(format == JpegFormat.Instance)	return FairMime.ImageJpg;
		if(format == PngFormat.Instance)	return FairMime.ImagePng;

		return FairMime.None;
	}
}
