import {
  AccountBase,
  AccountSearchLite,
  AuthorBaseAvatar,
  AuthorDetails,
  Category,
  CategoryBase,
  CategoryParentBase,
  CategoryPublications,
  File,
  Moderator,
  ModeratorProposal,
  PerpetualSurvey,
  PerpetualSurveyDetails,
  Policy,
  ProductDetails,
  ProductStore,
  Proposal,
  ProposalComment,
  ProposalDetails,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationChanged,
  PublicationChangedDetails,
  PublicationDetails,
  PublicationDetailsDiff,
  PublicationExtended,
  PublicationProposal,
  PublicationUnpublished,
  PublicationVersionInfo,
  Publisher,
  PublisherProposal,
  Review,
  ReviewProposal,
  Site,
  SiteBase,
  SiteLiteSearch,
  StatusResult,
  TotalItemsResult,
  User,
  UserAuthors,
  UserDetails,
  UserUnregistrationProposal,
} from "types"

export type FairApi = {
  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>
  getSitePolicies(siteId: string): Promise<Policy[]>
  getSiteUsers(siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<User>>
  getSitePublishers(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Publisher>>
  getSiteModerators(siteId: string): Promise<Moderator[]>
  getSiteFiles(siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>>
  searchSiteUsers(siteId: string, query?: string, limit?: number): Promise<User[]>

  searchAccounts(query?: string, limit?: number): Promise<AccountBase[]>
  searchAuthors(query?: string, limit?: number): Promise<AuthorBaseAvatar[]>
  searchSites(query?: string, page?: number): Promise<TotalItemsResult<SiteBase>>
  searchLiteSites(query?: string): Promise<SiteLiteSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<TotalItemsResult<PublicationExtended>>
  searchLitePublication(siteId: string, query?: string): Promise<PublicationBase[]>
  searchLiteAccounts(query?: string): Promise<AccountSearchLite[]>

  // User
  getUser(name: string): Promise<StatusResult<User>>
  getUserAuthors(userId: string): Promise<UserAuthors>
  getUserDetails(name: string): Promise<UserDetails>
  getUserSiteExists(userId: string, siteId: string): Promise<boolean>
  getUserReviews(userId: string, page?: number): Promise<TotalItemsResult<Review>>

  getAuthor(authorId: string): Promise<AuthorDetails>
  getCategoriesTree(siteId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategoryDetails(categoryId: string): Promise<Category>
  getCategoriesRoot(siteId: string): Promise<CategoryBase[]>
  getCategoriesPublications(siteId: string): Promise<CategoryPublications[]>
  getPublicationDetails(publicationId: string): Promise<PublicationDetails>
  getPublicationVersions(publicationId: string): Promise<PublicationVersionInfo>

  getChangedPublication(siteId: string, changedPublicationId: string): Promise<PublicationChangedDetails>
  getChangedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationChanged>>

  getUnpublishedSiteProduct(siteId: string, unpublishedProductId: string): Promise<ProductDetails>

  getUnpublishedPublication(siteId: string, publicationId: string): Promise<ProductDetails>
  getUnpublishedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationUnpublished>>

  getAuthorPublications(
    siteId: string,
    authorId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationAuthor>>
  getCategoryPublications(categoryId: string, page?: number): Promise<TotalItemsResult<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Review>>

  // Author
  getAuthorFiles(siteId: string, authorId?: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>>

  getAuthorPerpetualSurveys(siteId: string): Promise<PerpetualSurvey[]>
  getAuthorPerpetualSurveyDetails(siteId: string, perpetualSurveyId: string): Promise<PerpetualSurveyDetails>

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

  getModeratorUser(name: string): Promise<User>

  getProductDetails(productId: string): Promise<ProductDetails>
  getProductStores(productId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<ProductStore>>

  getPublicationDetailsDiff(publicationId: string, version: number): Promise<PublicationDetailsDiff>

  getModeratorProposals(
    siteId: string,
    search?: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ModeratorProposal>>

  getPublisherProposals(
    siteId: string,
    search?: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublisherProposal>>

  getReviewProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ReviewProposal>>

  getUserRegistrationProposals(siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Proposal>>

  getUserUnregistrationProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<UserUnregistrationProposal>>
}
