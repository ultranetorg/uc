import {
  Account,
  AccountBase,
  AccountSearchLite,
  AuthorBaseAvatar,
  AuthorDetails,
  BaseProposal,
  Category,
  CategoryParentBase,
  CategoryPublications,
  File,
  Moderator,
  ModeratorProposal,
  PerpetualSurvey,
  PerpetualSurveyDetails,
  Policy,
  ProductDetails,
  Proposal,
  ProposalComment,
  ProposalDetails,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationDetailsDiff,
  PublicationExtended,
  PublicationProposal,
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
  UnpublishedPublication,
  User,
} from "types"
import { ChangedPublication } from "types/ChangedPublication"
import { ChangedPublicationDetails } from "types/ChangedPublicationDetails"

export type Api = {
  getNexusUrl(): Promise<string>
  getVaultUrl(): Promise<string>

  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>
  getSitePolicies(siteId: string): Promise<Policy[]>
  getSitePublishers(siteId: string): Promise<Publisher[]>
  getSiteModerators(siteId: string): Promise<Moderator[]>
  getSiteFiles(siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>>

  searchAccounts(query?: string, limit?: number): Promise<AccountBase[]>
  searchAuthors(query?: string, limit?: number): Promise<AuthorBaseAvatar[]>
  searchSites(query?: string, page?: number): Promise<TotalItemsResult<SiteBase>>
  searchLiteSites(query?: string): Promise<SiteLiteSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<TotalItemsResult<PublicationExtended>>
  searchLitePublication(siteId: string, query?: string): Promise<PublicationBase[]>
  searchLiteAccounts(query?: string): Promise<AccountSearchLite[]>

  // User
  getUser(name: string): Promise<StatusResult<User>>
  getUserDetails(name: string): Promise<Account>

  getAuthor(authorId: string): Promise<AuthorDetails>
  getCategories(siteId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategory(categoryId: string): Promise<Category>
  getCategoriesPublications(siteId: string): Promise<CategoryPublications[]>
  getPublicationDetails(publicationId: string): Promise<PublicationDetails>
  getPublicationVersions(publicationId: string): Promise<PublicationVersionInfo>

  getChangedPublication(siteId: string, changedPublicationId: string): Promise<ChangedPublicationDetails>
  getChangedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ChangedPublication>>

  getUnpublishedSiteProduct(siteId: string, unpublishedProductId: string): Promise<ProductDetails>

  getUnpublishedPublication(siteId: string, publicationId: string): Promise<ProductDetails>
  getUnpublishedPublications(
    siteId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<UnpublishedPublication>>

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

  getUserProposals(
    siteId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<BaseProposal>>
}
