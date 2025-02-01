import { Author, Category, PaginationResponse, Publication, Site, SitePublication } from "types"

export type Api = {
  getAuthor(authorId: string): Promise<Author>
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<Publication>
  getSite(): Promise<Site>
  searchPublications(name?: string, page?: number, pageSize?: number): Promise<PaginationResponse<SitePublication>>
}
