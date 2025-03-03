import { Category, PaginationResponse, Publication, Site, SiteAuthor, SiteBase, SitePublication, User } from "types"

export type Api = {
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<Publication>
  getSite(siteId: string): Promise<Site>
  getSites(page?: number, pageSize?: number, name?: string): Promise<PaginationResponse<SiteBase>>
  getSiteAuthor(siteId: string, authorId: string): Promise<SiteAuthor>
  getUser(userId: string): Promise<User>
  searchPublications(
    siteId: string,
    name?: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<SitePublication>>
}
