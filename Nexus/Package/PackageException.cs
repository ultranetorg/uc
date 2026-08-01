namespace Uccs.Nexus;

public enum PackageError : byte
{
	None,

	IO,
	IncorrectContentType,
	NotSupportedReleaseAddressType
}

public class PackageException : CodeException
{
	public override int			Code { get => (int)Error; set => Error = (PackageError)value; }
	public PackageError			Error { get; protected set; }

	public PackageException()
	{
	}

	public PackageException(PackageError erorr, string message) : base($"{erorr} : {message}" )
	{
		Error = erorr;
	}
}
