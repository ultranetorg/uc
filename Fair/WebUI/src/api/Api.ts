import {
  AccountSearchLite,
  AuthorDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  ProductFieldCompare,
  ProductFieldModel,
  Proposal,
  ProposalComment,
  ProposalDetails,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationChanged,
  PublicationDetails,
  PublicationExtended,
  PublicationProposal,
  PublicationBaseSite,
  PublicationVersionInfo,
  Review,
  ReviewProposal,
  Site,
  SiteBase,
  SiteLiteSearch,
  TotalItemsResult,
  User,
  UserProposal,
  AccountBase,
  Product,
} from "types"

export type Api = {
  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>
  getSiteAuthors(siteId: string): Promise<AccountBase[]>
  getSiteModerators(siteId: string): Promise<AccountBase[]>
  getSiteFiles(siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<string>>

  searchAccounts(query?: string, limit?: number): Promise<AccountBase[]>
  searchSites(query?: string, page?: number): Promise<TotalItemsResult<SiteBase>>
  searchLiteSites(query?: string): Promise<SiteLiteSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<TotalItemsResult<PublicationExtended>>
  searchLitePublication(siteId: string, query?: string): Promise<PublicationBase[]>
  searchLiteAccounts(query?: string): Promise<AccountSearchLite[]>

  getAuthor(authorId: string): Promise<AuthorDetails>
  getCategories(siteId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategory(categoryId: string): Promise<Category>
  getCategoriesPublications(siteId: string): Promise<CategoryPublications[]>
  getPublication(publicationId: string): Promise<PublicationDetails>
  getPublicationVersions(publicationId: string): Promise<PublicationVersionInfo>
  getChangedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationChanged>>
  getUnpublishedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationBaseSite>>

  getAuthorPublications(
    siteId: string,
    authorId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationAuthor>>
  getCategoryPublications(categoryId: string, page?: number): Promise<TotalItemsResult<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Review>>
  getUser(userId: string): Promise<User>

  // Author
  getAuthorFiles(siteId: string, authorId?: string, page?: number, pageSize?: number): Promise<TotalItemsResult<string>>

  getAuthorReferendum(siteId: string, referendumId: string): Promise<ProposalDetails>
  getAuthorReferendums(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Proposal>>

  getAuthorReferendumComments(
    siteId: string,
    discussionId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ProposalComment>>

  getProduct(productId: string): Promise<Product>

  // Moderator
  getModeratorDiscussion(siteId: string, discussionId: string): Promise<ProposalDetails>
  getModeratorDiscussions(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Proposal>>

  getModeratorDiscussionComments(
    siteId: string,
    discussionId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ProposalComment>>

  getPublicationProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<PublicationProposal>>

  getProductFields(productId: string): Promise<ProductFieldModel[]>

  getProductCompareFields(publicationId: string, version: number): Promise<ProductFieldCompare>

  getReviewProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ReviewProposal>>

  getUserProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<UserProposal>>
}
