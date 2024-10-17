namespace UC.DomainModels;

public class ResourceData
{
	public Uccs.Rdn.DataType Type { get; set; }

	public byte[]? Value { get; set; }

	public int Length { get; set; }
}
