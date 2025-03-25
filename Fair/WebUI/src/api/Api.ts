import {
  AuthorReferendum,
  Category,
  ModeratorDispute,
  ModeratorPublication,
  ModeratorReview,
  PaginationResponse,
  Publication,
  Site,
  SiteAuthor,
  SiteBase,
  SitePublication,
  User,
} from "types"

export type Api = {
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<Publication>
  getSite(siteId: string): Promise<Site>
  getSites(page?: number, pageSize?: number, title?: string): Promise<PaginationResponse<SiteBase>>
  getSiteAuthor(siteId: string, authorId: string): Promise<SiteAuthor>
  getUser(userId: string): Promise<User>
  searchPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
    title?: string,
  ): Promise<PaginationResponse<SitePublication>>

  getAuthorReferendum(siteId: string, referendumId: string): Promise<AuthorReferendum>
  getAuthorReferendums(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<PaginationResponse<AuthorReferendum>>

  getModeratorDispute(siteId: string, disputeId: string): Promise<ModeratorDispute>
  getModeratorDisputes(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<PaginationResponse<ModeratorDispute>>
  getModeratorPublication(publicationId: string): Promise<ModeratorPublication>
  getModeratorPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<PaginationResponse<ModeratorPublication>>
  getModeratorReview(reviewId: string): Promise<ModeratorReview>
  getModeratorReviews(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<PaginationResponse<ModeratorReview>>
}
