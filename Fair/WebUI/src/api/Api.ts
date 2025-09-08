import {
  AuthorDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  Proposal,
  ProposalComment,
  ProposalDetails,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  PublicationProposal,
  Review,
  ReviewProposal,
  Site,
  SiteBase,
  SiteLiteSearch,
  TotalItemsResult,
  User,
  UserProposal,
} from "types"

export type Api = {
  getDefaultSites(): Promise<SiteBase[]>
  getSite(siteId: string): Promise<Site>

  searchSites(query?: string, page?: number): Promise<TotalItemsResult<SiteBase>>
  searchLiteSites(query?: string): Promise<SiteLiteSearch[]>
  searchPublications(siteId: string, query?: string, page?: number): Promise<TotalItemsResult<PublicationExtended>>
  searchLitePublication(siteId: string, query?: string): Promise<PublicationBase[]>

  getAuthor(authorId: string): Promise<AuthorDetails>
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

  // Author
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
