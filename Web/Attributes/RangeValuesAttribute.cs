namespace System.ComponentModel.DataAnnotations;

public class RangeValuesAttribute(int[] values) : ValidationAttribute
{
	public int[] Values { get; } = values;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		var intValue = value as int?;
		if (value == null || Values.All(x => x != intValue))
		{
			return new ValidationResult(null, [validationContext.MemberName]);
		}

		return ValidationResult.Success;
	}
}
