namespace Explorer.WebApi.Models.Responses;

public abstract class BaseRateResponse
{
	[JsonPropertyOrder(1)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public double RateUsd { get; set; } = default;

	[JsonPropertyOrder(2)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public double EmissionMultiplier { get; set; } = default;
}
