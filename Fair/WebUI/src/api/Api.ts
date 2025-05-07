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
  PaginationResult,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  Review,
  Site,
  SiteBase,
  SiteLightSearch,
  User,
} from "types"
import { CategoryPublications } from "types/CategoryPublications"

export type Api = {
  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>

  searchSites(query?: string, page?: number): Promise<PaginationResult<SiteBase>>
  searchLightSites(query?: string): Promise<SiteLightSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<PaginationResult<PublicationExtended>>
  searchLightPublication(siteId: string, query?: string): Promise<PublicationBase[]>

  getAuthor(authorId: string): Promise<Author>
  getCategories(siteId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategory(categoryId: string): Promise<Category>
  getCategoriesPublications(siteId: string): Promise<CategoryPublications[]>
  getPublication(publicationId: string): Promise<PublicationDetails>
  getAuthorPublications(
    siteId: string,
    authorId: string,
    page?: number,
    pageSize?: number,
  ): Promise<PaginationResponse<PublicationAuthor>>
  getCategoryPublications(categoryId: string, page?: number): Promise<PaginationResponse<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<PaginationResponse<Review>>
  getUser(userId: string): Promise<User>

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
