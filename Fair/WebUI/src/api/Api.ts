import {
  Author,
  AuthorReferendum,
  AuthorReferendumDetails,
  Category,
  ModeratorDispute,
  ModeratorDisputeDetails,
  ModeratorPublication,
  ModeratorReview,
  PaginationResponse,
  Publication,
  PublicationBase,
  Review,
  Site,
  SiteBase,
  User,
} from "types"

export type Api = {
  getAuthor(authorId: string): Promise<Author>
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<Publication>
  getAuthorPublications(
    siteId: string,
    authorId: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<PublicationBase>>
  getCategoryPublications(
    categoryId: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<PublicationBase>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<PaginationResponse<Review>>
  getSite(siteId: string): Promise<Site>
  getSites(page?: number, pageSize?: number, search?: string): Promise<PaginationResponse<SiteBase>>
  getUser(userId: string): Promise<User>
  searchPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
    title?: string,
  ): Promise<PaginationResponse<Publication>>

  getAuthorReferendum(siteId: string, referendumId: string): Promise<AuthorReferendumDetails>
  getAuthorReferendums(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<PaginationResponse<AuthorReferendum>>

  getModeratorDispute(siteId: string, disputeId: string): Promise<ModeratorDisputeDetails>
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
