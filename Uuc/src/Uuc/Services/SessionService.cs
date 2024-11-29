using System.Timers;

using Timer = System.Timers.Timer;

namespace Uuc.Services;

public class SessionService : ISessionService
{
	private readonly Timer _inactivityTimer;
	private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(5); // Тайм-аут бездействия

	public event Action? SessionExpired;

	public SessionService()
	{
		_inactivityTimer = new Timer(_sessionTimeout.TotalMilliseconds);
		_inactivityTimer.Elapsed += OnSessionExpired;
		_inactivityTimer.AutoReset = false; // Таймер срабатывает только один раз
	}

	public void StartSession()
	{
		ResetSessionTimer();
	}

	public void ExtendSessionIfActive()
	{
		ResetSessionTimer();
	}

	public void EndSession()
	{
		_inactivityTimer.Stop();
		SessionExpired?.Invoke();
	}

	private void ResetSessionTimer()
	{
		_inactivityTimer.Stop();
		_inactivityTimer.Start();
	}

	private void OnSessionExpired(object? sender, ElapsedEventArgs e)
	{
		SessionExpired?.Invoke();
	}
}
