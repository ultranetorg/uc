﻿namespace Uccs.Fair;

public class PublicationUpdationModel(PublicationUpdation operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();
	public ProductFieldVersionReferenceModel Change { get; set; } = new ProductFieldVersionReferenceModel(operation.Change);
}
