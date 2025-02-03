import { Author, Category, PaginationResponse, Publication, Site, SitePublication, User } from "types"

export type Api = {
  getAuthor(authorId: string): Promise<Author>
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<Publication>
  getSite(): Promise<Site>
  getUser(userId: string): Promise<User>
  searchPublications(name?: string, page?: number, pageSize?: number): Promise<PaginationResponse<SitePublication>>
}
