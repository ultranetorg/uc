namespace Uuc.Services;

public interface IApplicationService
{
	Task DisplayAlert(string title, string message, string cancel);
}

