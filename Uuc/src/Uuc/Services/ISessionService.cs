using System.Diagnostics.CodeAnalysis;

namespace Uuc.Services;

public interface ISessionService
{
	string? SessionId { get; }

	event Action? SessionExpired;

	bool IsSessionValid([NotEmpty] string sessionId);

	void StartSession();

	void ExtendSessionIfActive([NotEmpty] string sessionId);

	void EndSession();
}
