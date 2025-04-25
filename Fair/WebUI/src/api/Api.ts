import {
  Author,
  AuthorReferendum,
  AuthorReferendumDetails,
  Category,
  CategoryParentBase,
  ModeratorDispute,
  ModeratorDisputeDetails,
  ModeratorPublication,
  ModeratorReview,
  PaginationResponse,
  Publication,
  PublicationAuthor,
  PublicationDetails,
  PublicationSearch,
  Review,
  Site,
  SiteBase,
  User,
} from "types"

export type Api = {
  getAuthor(authorId: string): Promise<Author>
  getCategories(siteId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategory(categoryId: string): Promise<Category>
  getPublication(publicationId: string): Promise<PublicationDetails>
  getAuthorPublications(
    siteId: string,
    authorId: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<PublicationAuthor>>
  getCategoryPublications(
    categoryId: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<PaginationResponse<Review>>
  getSite(siteId: string): Promise<Site>
  getSites(page?: number, pageSize?: number, search?: string): Promise<PaginationResponse<SiteBase>>
  getUser(userId: string): Promise<User>
  searchPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
    title?: string,
  ): Promise<PaginationResponse<PublicationSearch>>

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
