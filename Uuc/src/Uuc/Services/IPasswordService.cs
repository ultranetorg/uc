using System.Diagnostics.CodeAnalysis;

namespace Uuc.Services;

public interface IPasswordService
{
	string? Password { get; set; }

	Task<bool> IsValidAsync([NotEmpty] string password);

	Task SaveHashAsync([NotEmpty] string password);

	Task<bool> IsHashSavedAsync();
}
