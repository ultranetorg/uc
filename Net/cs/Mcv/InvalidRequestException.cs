namespace Uccs.Net;

class InvalidRequestException : Exception
{
	public InvalidRequestException(string message) : base(message)
	{
	}
}
