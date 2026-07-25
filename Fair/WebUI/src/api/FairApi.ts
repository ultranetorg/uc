import {
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
  ProductAuthor,
  ProductDetails,
  ProductSearchResult,
  ProductSearchResultBase,
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
  StatusResult,
  Store,
  StoreBase,
  StoreLiteSearch,
  TotalItemsResult,
  User,
  UserAuthors,
  UserBase,
  UserDetails,
  UserUnregistrationProposal,
} from "types"

export type FairApi = {
  getDefaultStores(): Promise<StoreBase[]>
  getStore(storeId: string): Promise<Store>
  getStorePolicies(storeId: string): Promise<Policy[]>
  getStoreUsers(storeId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<User>>
  getStorePublishers(
    storeId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Publisher>>
  getStoreModerators(storeId: string): Promise<Moderator[]>
  getStoreFiles(storeId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>>
  searchStoreUsers(storeId: string, query?: string, limit?: number): Promise<User[]>

  searchUsers(query?: string, limit?: number): Promise<UserBase[]>
  searchAuthors(query?: string, limit?: number): Promise<AuthorBaseAvatar[]>
  searchStores(query?: string, page?: number): Promise<TotalItemsResult<StoreBase>>
  searchLiteStores(query?: string): Promise<StoreLiteSearch[]>
  searchPublications(storeId: string, query?: string, page?: number): Promise<PublicationExtended[]>
  searchLitePublication(storeId: string, query?: string): Promise<PublicationBase[]>
  searchLiteProducts(query?: string): Promise<ProductSearchResultBase[]>
  searchProducts(query?: string, page?: number, pageSize?: number): Promise<ProductSearchResult[]>

  // User
  getUser(name: string): Promise<StatusResult<User>>
  getUserAuthors(userId: string): Promise<UserAuthors>
  getUserDetails(name: string): Promise<UserDetails>
  getUserStoreExists(userId: string, storeId: string): Promise<boolean>
  getUserReviews(userId: string, page?: number): Promise<TotalItemsResult<Review>>

  getCategoriesTree(storeId: string, depth?: number): Promise<CategoryParentBase[]>
  getCategoryDetails(categoryId: string): Promise<Category>
  getCategoriesRoot(storeId: string): Promise<CategoryBase[]>
  getCategoriesPublications(storeId: string): Promise<CategoryPublications[]>
  getPublicationDetails(publicationId: string): Promise<PublicationDetails>
  getPublicationVersions(publicationId: string): Promise<PublicationVersionInfo>

  getChangedPublication(storeId: string, changedPublicationId: string): Promise<PublicationChangedDetails>
  getChangedPublications(
    storeId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationChanged>>

  getUnpublishedStoreProduct(storeId: string, unpublishedProductId: string): Promise<ProductDetails>

  getUnpublishedPublication(storeId: string, publicationId: string): Promise<ProductDetails>
  getUnpublishedPublications(
    storeId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationUnpublished>>

  getPublisherPublications(
    storeId: string,
    publisherId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublicationAuthor>>
  getCategoryPublications(categoryId: string, page?: number): Promise<TotalItemsResult<Publication>>
  getReviews(publicationId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Review>>

  // Author
  getAuthor(authorId: string): Promise<AuthorDetails>
  getAuthorProducts(authorId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<ProductAuthor>>

  getAuthorFiles(storeId: string, authorId?: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>>

  getAuthorPerpetualSurveys(storeId: string): Promise<PerpetualSurvey[]>
  getAuthorPerpetualSurveyDetails(storeId: string, perpetualSurveyId: string): Promise<PerpetualSurveyDetails>

  getAuthorReferendum(storeId: string, referendumId: string): Promise<ProposalDetails>
  getAuthorReferendums(
    storeId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Proposal>>

  getAuthorReferendumComments(
    storeId: string,
    discussionId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ProposalComment>>

  // Moderator
  getModeratorDiscussion(storeId: string, discussionId: string): Promise<ProposalDetails>
  getModeratorDiscussions(
    storeId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<Proposal>>

  getModeratorDiscussionComments(
    storeId: string,
    discussionId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ProposalComment>>

  getPublicationProposals(
    storeId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<PublicationProposal>>

  getModeratorUser(name: string): Promise<User>

  getProductDetails(productId: string): Promise<ProductDetails>
  getProductStores(productId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<ProductStore>>

  getPublicationDetailsDiff(publicationId: string, version: number): Promise<PublicationDetailsDiff>

  getModeratorProposals(
    storeId: string,
    search?: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<ModeratorProposal>>

  getPublisherProposals(
    storeId: string,
    search?: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<PublisherProposal>>

  getReviewProposals(
    storeId: string,
    page?: number,
    pageSize?: number,
    search?: string,
  ): Promise<TotalItemsResult<ReviewProposal>>

  getUserRegistrationProposals(storeId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<Proposal>>

  getUserUnregistrationProposals(
    storeId: string,
    page?: number,
    pageSize?: number,
  ): Promise<TotalItemsResult<UserUnregistrationProposal>>
}
