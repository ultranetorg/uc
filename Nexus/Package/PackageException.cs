namespace Uccs.Nexus;

public enum PackageError : byte
{
	None,

	IO,
	IncorrectContentType,
	NotFound,
	NotSupportedReleaseAddressType,
	MissingResource,
	NotSupported,
	ParentPackagesNotFound,
	Busy,
}

public class PackageException : CodeException
{
	public override int			Code { get => (int)Error; set => Error = (PackageError)value; }
	public PackageError			Error { get; protected set; }

	public PackageException()
	{
	}

	public PackageException(PackageError erorr, string message = null) : base(message == null ? erorr.ToString() : $"{erorr} : {message}" )
	{
		Error = erorr;
	}
}
