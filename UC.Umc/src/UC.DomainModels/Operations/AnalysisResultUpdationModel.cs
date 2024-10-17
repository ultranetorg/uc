namespace UO.DomainModels.Operations;

public class AnalysisResultUpdationModel : BaseOperationModel
{
	public string ResourceId { get; set; } = null!;

	public Uccs.Rdn.AnalysisResult Result { get; set; }
}
