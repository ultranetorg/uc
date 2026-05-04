namespace Uccs.Fair;

public static class EnumExtensions
{
	public static IEnumerable<T> GetFlags<T>(this T value) where T : Enum
	{
		foreach(var flag in Enum.GetValues(typeof(T)).Cast<T>())
		{
			if(Convert.ToInt64(flag) == 0) continue;

			if(value.HasFlag(flag))
				yield return flag;
		}
	}
}
