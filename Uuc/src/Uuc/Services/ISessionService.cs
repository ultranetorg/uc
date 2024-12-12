namespace Uuc.Services;

public interface ISessionService
{
	string? SessionId { get; }

	event Action? SessionExpired;

	void StartSession();

	void ExtendSessionIfActive();

	void EndSession();
}
