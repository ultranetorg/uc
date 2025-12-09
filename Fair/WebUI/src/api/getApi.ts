import { DEFAULT_PAGE_SIZE_20 } from "config"
import { LIMIT_DEFAULT } from "constants/"
import {
  Account,
  AccountBase,
  AccountSearchLite,
  AuthorDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  ChangedPublication,
  ChangedPublicationDetails,
  ProductFieldCompare,
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
  UnpublishedProduct,
  UnpublishedProductDetails,
  User,
  UserProposal,
} from "types"

import { Api } from "./Api"
import { buildUrlParams, toTotalItemsResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getNexusUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/nexus`).then(res => res.json())

const getVaultUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/vault`).then(res => res.json())

const getDefaultSites = (): Promise<SiteBase[]> => fetch(`${BASE_URL}/sites/default`).then(res => res.json())

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const getSiteAuthors = (siteId: string): Promise<AccountBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/authors`).then(res => res.json())

const getSiteModerators = (siteId: string): Promise<AccountBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/moderators`).then(res => res.json())

const getSiteFiles = async (siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<string>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/files` + params)
  return await toTotalItemsResult(res)
}

const searchAccounts = (query?: string, limit?: number): Promise<AccountBase[]> =>
  fetch(`${BASE_URL}/accounts?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res => res.json())

const searchSites = async (query?: string, page?: number): Promise<TotalItemsResult<SiteBase>> => {
  const params = buildUrlParams({ query, page })
  const res = await fetch(`${BASE_URL}/sites` + params)
  return await toTotalItemsResult(res)
}

const searchLiteSites = (query?: string): Promise<SiteLiteSearch[]> =>
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

const getAccountByAddress = (accountAddress: string): Promise<Account> =>
  fetch(`${BASE_URL}/accounts/address/${accountAddress}`).then(res => res.json())

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

const getChangedPublication = (siteId: string, changedPublicationId: string): Promise<ChangedPublicationDetails> =>
  fetch(`${BASE_URL}/sites/${siteId}/publications/changed/${changedPublicationId}`).then(res => res.json())

const getChangedPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ChangedPublication>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications/changed` + params)
  return await toTotalItemsResult(res)
}

const headUnpublishedProduct = async (unpublishedProductId: string): Promise<boolean> => {
  const response = await fetch(`${BASE_URL}/products/unpublished/${unpublishedProductId}`, { method: "HEAD" })
  return response.status === 200
}

const getUnpublishedProduct = (unpublishedProductId: string): Promise<UnpublishedProductDetails> =>
  fetch(`${BASE_URL}/products/unpublished/${unpublishedProductId}`).then(res => res.json())

const getUnpublishedSiteProduct = (siteId: string, unpublishedProductId: string): Promise<UnpublishedProductDetails> =>
  fetch(`${BASE_URL}/sites/${siteId}/products/unpublished/${unpublishedProductId}`).then(res => res.json())

const getUnpublishedSiteProducts = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<UnpublishedProduct>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/products/unpublished` + params)
  return await toTotalItemsResult(res)
}

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

const getAuthorFiles = async (
  siteId: string,
  authorId?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<string>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/authors/${authorId}/files` + params)
  return await toTotalItemsResult(res)
}

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

const getModeratorDiscussion = (siteId: string, discussionId: string): Promise<ProposalDetails> =>
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

const getProductFields = (productId: string): Promise<ProductFieldModel[]> =>
  fetch(`${BASE_URL}/products/${productId}/fields`).then(res => res.json())

const getProductCompareFields = (publicationId: string, version: number): Promise<ProductFieldCompare> =>
  fetch(`${BASE_URL}/publications/${publicationId}/updated-fields?version=${version}`).then(res => res.json())

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
  getNexusUrl,
  getVaultUrl,

  getAccountByAddress,
  getAuthor,
  getAuthorPublications,
  getCategories,
  getCategoriesPublications,
  getCategory,
  getCategoryPublications,
  getDefaultSites,
  getPublication,
  getPublicationVersions,

  getChangedPublication,
  getChangedPublications,
  headUnpublishedProduct,
  getUnpublishedProduct,
  getUnpublishedSiteProduct,
  getUnpublishedSiteProducts,

  getReviews,
  getSite,
  getSiteAuthors,
  getSiteFiles,
  getSiteModerators,
  getUser,
  searchLitePublication,
  searchLiteSites,
  searchPublications,
  searchAccounts,
  searchSites,
  searchLiteAccounts,

  getAuthorFiles,
  getAuthorReferendum,
  getAuthorReferendums,
  getAuthorReferendumComments,

  getModeratorDiscussion,
  getModeratorDiscussions,
  getModeratorDiscussionComments,
  getPublicationProposals,
  getProductFields,
  getProductCompareFields,
  getReviewProposals,
  getUserProposals,
}

export const getApi = () => api
