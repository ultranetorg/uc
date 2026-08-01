namespace Uccs.Fair;

public enum ProductError : byte
{
	None,
	UnknownDataType,
	UnknownAddressType,
	BothProductAndReleaseNotFound,
	RequiredPackagesNotFound,
	AlreadyExists,
	NotSupportedDataType,
	Busy,
	NotFound,
	HashMismatch,
	//DownloadFailed
}

public class ProductException : CodeException
{
	public override int				Code { get => (int)Error; set => Error = (ProductError)value; }
	public ProductError				Error { get; protected set; }

	public ProductException()
	{
	}

	public ProductException(ProductError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
