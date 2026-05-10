namespace Uccs.Net;

public class Wayin : IBinarySerializable
{
	public string	Software { get; set; }	/// iccp:rdn/fns, 
											///	https://company.com/installer.exe
											
	public string	Arguments { get; set; } /// https://fair.net

	public void Read(Reader reader)
	{
		Software = reader.ReadUtf8();
		Arguments = reader.ReadUtf8();
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Software);
		writer.WriteUtf8(Arguments);
	}
}

public class InfoIcca : IccpArgumentation
{
}

public class InfoIccr : IccpResult
{
	public Wayin[]			Wayins { get; set; }

	public override void	Read(Reader reader) => Wayins = reader.ReadArray<Wayin>();
	public override void	Write(Writer writer) => writer.Write(Wayins);
}
