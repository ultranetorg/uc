﻿namespace Uccs.Fair;

public class PublicationModel
{
	public string Id { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	public string ProductId { get; set; }
	public string ProductTitle { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }

	public PublicationModel(Publication publication, Category category, Author author, Product product)
	{
		Id = publication.Id.ToString();

		CategoryId = category.Id.ToString();
		CategoryTitle = category.Title;

		ProductId = product.Id.ToString();
		ProductTitle = ProductUtils.GetTitle(product, publication);

		AuthorId = author?.Id.ToString();
		AuthorTitle = author?.Title;
	}
}
