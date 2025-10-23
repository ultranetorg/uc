import { DEFAULT_PAGE_SIZE_20 } from "config"
import {
  AccountSearchLite,
  AuthorDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  ProductFieldModel,
  Proposal,
  ProposalComment,
  ProposalDetails,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  PublicationProposal,
  PublicationVersionInfo,
  Review,
  ReviewProposal,
  Site,
  SiteBase,
  SiteLiteSearch,
  TotalItemsResult,
  User,
  UserProposal,
} from "types"

import { Api } from "./Api"
import { buildUrlParams, toTotalItemsResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getDefaultSites = (): Promise<SiteBase[]> => fetch(`${BASE_URL}/sites/default`).then(res => res.json())

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const searchSites = async (query?: string, page?: number): Promise<TotalItemsResult<SiteBase>> => {
  const params = buildUrlParams({ query, page })
  const res = await fetch(`${BASE_URL}/sites` + params)
  return await toTotalItemsResult(res)
}

const searchLiteSites = async (query?: string): Promise<SiteLiteSearch[]> =>
  fetch(`${BASE_URL}/sites/search?query=${query}`).then(res => res.json())

const searchPublications = async (
  siteId: string,
  query?: string,
  page?: number,
): Promise<TotalItemsResult<PublicationExtended>> => {
  const params = buildUrlParams({ query, page })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications` + params)
  return await toTotalItemsResult(res)
}

const searchLitePublication = (siteId: string, query?: string): Promise<PublicationBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/publications/search?query=${query}`).then(res => res.json())

const searchLiteAccounts = (query?: string): Promise<AccountSearchLite[]> =>
  fetch(`${BASE_URL}/accounts/search?query=${query}`).then(res => res.json())

const getAuthor = (authorId: string): Promise<AuthorDetails> =>
  fetch(`${BASE_URL}/authors/${authorId}`).then(res => res.json())

const getCategories = (siteId: string, depth?: number): Promise<CategoryParentBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/categories` + (depth !== undefined ? `?depth=${depth}` : "")).then(res =>
    res.json(),
  )

const getCategory = (categoryId: string): Promise<Category> =>
  fetch(`${BASE_URL}/categories/${categoryId}`).then(res => res.json())

const getCategoriesPublications = (siteId: string): Promise<CategoryPublications[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/categories/publications`).then(res => res.json())

const getPublication = (publicationId: string): Promise<PublicationDetails> =>
  fetch(`${BASE_URL}/publications/${publicationId}`).then(res => res.json())

const getPublicationVersions = (publicationId: string): Promise<PublicationVersionInfo> =>
  fetch(`${BASE_URL}/publications/${publicationId}/versions`).then(res => res.json())

const getAuthorPublications = async (
  siteId: string,
  authorId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationAuthor>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/authors/${authorId}/publications` + params)
  return await toTotalItemsResult(res)
}

const getCategoryPublications = async (categoryId: string, page?: number): Promise<TotalItemsResult<Publication>> => {
  const res = await fetch(
    `${BASE_URL}/categories/${categoryId}/publications${page && page > 0 ? "?page=" + page.toString() : ""}`,
  )
  return await toTotalItemsResult(res)
}

const getReviews = async (
  publicationId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<Review>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/publications/${publicationId}/reviews` + params)
  return await toTotalItemsResult(res)
}

const getUser = (userId: string): Promise<User> => fetch(`${BASE_URL}/users/${userId}`).then(res => res.json())

const getAuthorReferendum = (siteId: string, referendumId: string): Promise<ProposalDetails> =>
  fetch(`${BASE_URL}/author/sites/${siteId}/referendums/${referendumId}`).then(res => res.json())

const getAuthorReferendumComments = async (
  siteId: string,
  referendumId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProposalComment>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/author/sites/${siteId}/referendums/${referendumId}/comments` + params)
  return await toTotalItemsResult(res)
}

const getAuthorReferendums = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/author/sites/${siteId}/referendums` + params)
  return await toTotalItemsResult(res)
}

const getFile = (id: string): Promise<string> => fetch(`${BASE_URL}/files/${id}`).then(res => res.json())

const getModeratorDiscussion = async (siteId: string, discussionId: string): Promise<ProposalDetails> =>
  fetch(`${BASE_URL}/moderator/sites/${siteId}/discussions/${discussionId}`).then(res => res.json())

const getModeratorDiscussionComments = async (
  siteId: string,
  discussionId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProposalComment>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/discussions/${discussionId}/comments` + params)
  return await toTotalItemsResult(res)
}

const getModeratorDiscussions = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/discussions` + params)
  return await toTotalItemsResult(res)
}

const getPublicationProposals = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<PublicationProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/publications` + params)
  return await toTotalItemsResult(res)
}

const getProductFields = async (
  productId: string,
): Promise<TotalItemsResult<ProductFieldModel>> => {
  const res = await fetch(`${BASE_URL}/products/${productId}/fields`)
  return await toTotalItemsResult(res)
}

const getReviewProposals = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<ReviewProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/reviews` + params)
  return await toTotalItemsResult(res)
}

const getUserProposals = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<UserProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/users` + params)
  return await toTotalItemsResult(res)
}

const api: Api = {
  getAuthor,
  getAuthorPublications,
  getCategories,
  getCategoriesPublications,
  getCategory,
  getCategoryPublications,
  getDefaultSites,
  getPublication,
  getPublicationVersions,
  getReviews,
  getSite,
  getUser,
  getFile,
  searchLitePublication,
  searchLiteSites,
  searchPublications,
  searchSites,
  searchLiteAccounts,

  getAuthorReferendum,
  getAuthorReferendums,
  getAuthorReferendumComments,

  getModeratorDiscussion,
  getModeratorDiscussions,
  getModeratorDiscussionComments,
  getPublicationProposals,
  getProductFields,
  getReviewProposals,
  getUserProposals,
}

export const getApi = () => api
