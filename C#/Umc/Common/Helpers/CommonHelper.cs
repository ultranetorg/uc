using System.Linq.Expressions;
using System.Security.Cryptography;

namespace UC.Umc.Helpers;

public static class CommonHelper
{
	public static int GetDaysLeft(DateTime date) =>
		date == default ? 0
		: (int)(new TimeSpan(date.Ticks - DateTime.Now.Ticks).TotalDays);

	// WBD
    public static string GenerateUniqueID(int length)
    {
        int sufficientBufferSizeInBytes = (length * 6 + 7) / 8;
        var buffer = new byte[sufficientBufferSizeInBytes];
        RandomNumberGenerator.Create().GetBytes(buffer);
        return Convert.ToBase64String(buffer)[..length];
    }

    public static async Task<byte[]> ReadFile(string path)
    {
		byte[] result = null;
        try
        {
            using (var stream = File.OpenWrite(path))
            using (var newStream = new MemoryStream())
            {
                await stream.CopyToAsync(newStream);
                result = newStream.ToArray();
            }
        }
        catch(Exception ex)
        {
            await ToastHelper.ShowMessageAsync("Loading error");
			ThrowHelper.ThrowInvalidOperationException("PathToBytes: Loading error", ex);
        }
        return result;
    }

    /// <summary>
    /// Awaits <c>Task</c>. Exceptions are handled by <c>errorAction</c>
    /// </summary>
    public static async void AwaitTaskAsync(this Task task, Action<Exception> errorAction, bool configureAwait = true)
    {
        try
        {
            await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            errorAction?.Invoke(ex);
        }
    }
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortExpression) where T : class
    {
        if (!string.IsNullOrWhiteSpace(sortExpression))
        {
            sortExpression = sortExpression.Trim();
            var capitalFirstLetter = sortExpression[0].ToString().ToUpper();
            sortExpression = capitalFirstLetter + ((sortExpression.Length > 1) ? sortExpression.Substring(1) : string.Empty);
        }

        var orderByProperty = string.Empty;

        sortExpression += "";
        var parts = sortExpression.Split(' ');
        var descending = false;

        if (parts.Length > 0 && parts[0] != "")
        {
            orderByProperty = parts[0];

            if (parts.Length > 1)
            {
                descending = parts[1].ToLower().Contains("esc");
            }
        }

        if (string.IsNullOrWhiteSpace(orderByProperty))
        {
            return source;
        }

        var command = descending ? "OrderByDescending" : "OrderBy";
        var type = typeof(T);
        var property = type.GetProperty(orderByProperty);
        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var resultExpression = Expression.Call(typeof(Queryable), command, new[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExpression));
        return source.Provider.CreateQuery<T>(resultExpression);
    }
}