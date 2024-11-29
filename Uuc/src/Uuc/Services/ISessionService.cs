namespace Uuc.Services;

public interface ISessionService
{
	void StartSession();

	void ExtendSessionIfActive();

	void EndSession();
}
