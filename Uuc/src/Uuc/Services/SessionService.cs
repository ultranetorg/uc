using System.Timers;

using Timer = System.Timers.Timer;

namespace Uuc.Services;

public class SessionService : ISessionService
{
	private readonly Timer _inactivityTimer;
	private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(5);

	public string? SessionId { get; private set; }

	public event Action? SessionExpired;

	public SessionService()
	{
		_inactivityTimer = new Timer(_sessionTimeout.TotalMilliseconds);
		_inactivityTimer.Elapsed += OnSessionExpired;
		_inactivityTimer.AutoReset = false;
	}

	public void StartSession()
	{
		GenerateSessionId();
		ResetSessionTimer();
	}

	public void ExtendSessionIfActive()
	{
		ResetSessionTimer();
	}

	public void EndSession()
	{
		SessionId = null;
		_inactivityTimer.Stop();
		SessionExpired?.Invoke();
	}

	private void GenerateSessionId()
	{
		SessionId = Guid.NewGuid().ToString();
	}

	private void ResetSessionTimer()
	{
		_inactivityTimer.Stop();
		_inactivityTimer.Start();
	}

	private void OnSessionExpired(object? sender, ElapsedEventArgs e)
	{
		EndSession();
	}
}
