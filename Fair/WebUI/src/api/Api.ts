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
  PaginationResult,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  Review,
  Site,
  SiteBase,
  SiteLiteSearch,
  TotalItemsResult,
  User,
} from "types"
import { CategoryPublications } from "types/CategoryPublications"

export type Api = {
  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>

  searchSites(query?: string, page?: number): Promise<PaginationResult<SiteBase>>
  searchLiteSites(query?: string): Promise<SiteLiteSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<PaginationResult<PublicationExtended>>
  searchLitePublication(siteId: string, query?: string): Promise<PublicationBase[]>

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
  ): Promise<TotalItemsResult<PublicationAuthor>>
  getCategoryPublications(categoryId: string, page?: number): Promise<TotalItemsResult<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Review>>
  getUser(userId: string): Promise<User>

  getAuthorReferendum(siteId: string, referendumId: string): Promise<AuthorReferendumDetails>
  getAuthorReferendums(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<AuthorReferendum>>

  getModeratorDispute(siteId: string, disputeId: string): Promise<ModeratorDisputeDetails>
  getModeratorDisputes(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ModeratorDispute>>
  getModeratorPublication(publicationId: string): Promise<ModeratorPublication>
  getModeratorPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ModeratorPublication>>
  getModeratorReview(reviewId: string): Promise<ModeratorReview>
  getModeratorReviews(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ModeratorReview>>
}
