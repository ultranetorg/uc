using System.Diagnostics.CodeAnalysis;

namespace Uccs.WebUI.Utilities;

public class Result<T>([NotNull] T? value)
{
	public T? Value { get; } = value;
}

public class Condition
{
	public bool Value { get; }

	public Condition(bool value)
	{
		Value = value;
	}

	public void Throw<T>() where T : Exception, new()
	{
		if (Value)
		{
			throw new T();
		}
	}

	public void Throw<T>(Func<T> action) where T : Exception
	{
		if (Value)
		{
			throw action();
		}
	}
}

public class If
{
	public static Result<bool> Value(bool value) => new(value);

	public static Result<int> Value(int value) => new(value);

	public static Result<long> Value(long value) => new(value);

	public static Result<object?> Value([NotNull] object? value) => new(value);

	public static Result<string?> Value([NotNull] string? value) => new(value);
}
