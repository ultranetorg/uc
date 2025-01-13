namespace Uccs.Rdn;

public enum ResourceError : byte
{
	None,
	UnknownDataType,
	UnknownAddressType,
	BothResourceAndReleaseNotFound,
	RequiredPackagesNotFound,
	AlreadyExists,
	NotSupportedDataControl,
	NotSupportedDataType,
	Busy,
	NotFound,
	NoData,
	HashMismatch,
	//DownloadFailed
}

public class ResourceException : NetException
{
	public override int				ErrorCode { get => (int)Error; set => Error = (ResourceError)value; }
	public ResourceError			Error { get; protected set; }
	public override string			Message => Error.ToString();

	public ResourceException()
	{
	}

	public ResourceException(ResourceError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
