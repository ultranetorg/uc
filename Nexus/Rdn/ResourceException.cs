namespace Uccs.Rdn;

public enum ResourceError : byte
{
	None,

	AlreadyExists,
	BothResourceAndReleaseNotFound,
	Busy,
	HashMismatch,
	InvalidMeaning,
	NoData,
	NotFound,
	NotHub,
	NotSupportedDataType,
	ParentPackagesNotFound,
	UnknownDataType,
	UnknownAddressType,
}

public class ResourceException : CodeException
{
	public override int				Code { get => (int)Error; set => Error = (ResourceError)value; }
	public ResourceError			Error { get; protected set; }

	public ResourceException()
	{
	}

	public ResourceException(ResourceError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
