import { DEFAULT_PAGE_SIZE_20 } from "config"
import { LIMIT_DEFAULT } from "constants/"
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
  ChangedPublication,
  ChangedPublicationDetails,
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

import { Api } from "./Api"
import { buildUrlParams, toTotalItemsResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getNexusUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/nexus`).then(res => res.json())

const getVaultUrl = (): Promise<string> => fetch(`${BASE_URL}/node/urls/vault`).then(res => res.json())

const getDefaultSites = (): Promise<SiteBase[]> => fetch(`${BASE_URL}/sites/default`).then(res => res.json())

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const getSitePolicies = (siteId: string): Promise<Policy[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/policies`).then(res => res.json())

const getSitePublishers = (siteId: string): Promise<Publisher[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/publishers`).then(res => res.json())

const getSiteModerators = (siteId: string): Promise<Moderator[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/moderators`).then(res => res.json())

const getSiteFiles = async (siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/files` + params)
  if (!res.ok) throw new Error(`Failed to fetch site files`)
  return await toTotalItemsResult(res)
}

const searchAccounts = (query?: string, limit?: number): Promise<AccountBase[]> =>
  fetch(`${BASE_URL}/accounts?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res => res.json())

const searchAuthors = (query?: string, limit?: number): Promise<AuthorBaseAvatar[]> =>
  fetch(`${BASE_URL}/authors?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res => res.json())

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

const getUser = async (name: string): Promise<StatusResult<User>> => {
  try {
    const res = await fetch(`${BASE_URL}/users/${name}`)
    if (!res.ok) {
      return { ok: res.ok, status: res.status }
    }

    const data: User = await res.json()
    return { ok: res.ok, status: res.status, data }
  } catch (error) {
    console.error(error)
    return { ok: false, status: 0 } // 0 = network / unknown error
  }
}

const getUserDetails = (name: string): Promise<Account> =>
  fetch(`${BASE_URL}/users/${name}/details`).then(res => res.json())

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

const getUnpublishedSiteProduct = async (siteId: string, unpublishedProductId: string): Promise<ProductDetails> => {
  const res = await fetch(`${BASE_URL}/sites/${siteId}/products/unpublished/${unpublishedProductId}`)
  if (!res.ok) {
    throw new Error()
  }

  return await res.json()
}

const getUnpublishedPublication = (siteId: string, publicationId: string): Promise<ProductDetails> =>
  fetch(`${BASE_URL}/sites/${siteId}/publications/unpublished/${publicationId}`).then(res => res.json())

const getUnpublishedPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<UnpublishedPublication>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications/unpublished` + params)
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

const getAuthorFiles = async (
  siteId: string,
  authorId?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<File>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/authors/${authorId}/files` + params)
  if (!res.ok) throw new Error(`Failed to fetch author files`)
  return await toTotalItemsResult(res)
}

const getAuthorPerpetualSurveys = (siteId: string): Promise<PerpetualSurvey[]> =>
  fetch(`${BASE_URL}/author/sites/${siteId}/perpetual-surveys`).then(res => res.json())

const getAuthorPerpetualSurveyDetails = (siteId: string, perpetualSurveyId: string): Promise<PerpetualSurveyDetails> =>
  fetch(`${BASE_URL}/author/sites/${siteId}/perpetual-surveys/${perpetualSurveyId}`).then(res => res.json())

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

const getModeratorDiscussion = async (siteId: string, discussionId: string): Promise<ProposalDetails> => {
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/discussions/${discussionId}`)
  if (!res.ok) {
    throw new Error()
  }

  return await res.json()
}

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

const getModeratorUser = (name: string): Promise<User> =>
  fetch(`${BASE_URL}/moderator/users/${name}`).then(res => res.json())

const getProductDetails = (productId: string): Promise<ProductDetails> =>
  fetch(`${BASE_URL}/products/${productId}`).then(res => res.json())

const getPublicationDetailsDiff = (publicationId: string, version: number): Promise<PublicationDetailsDiff> =>
  fetch(`${BASE_URL}/publications/${publicationId}/diff?to=${version}`).then(res => res.json())

const getModeratorProposals = async (
  siteId: string,
  search?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ModeratorProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/proposals/moderators` + params)
  return await toTotalItemsResult(res)
}

const getPublisherProposals = async (
  siteId: string,
  search?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublisherProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/proposals/publishers` + params)
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
): Promise<TotalItemsResult<BaseProposal>> => {
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

  getUser,
  getUserDetails,

  getAuthor,
  getAuthorPublications,
  getCategories,
  getCategoriesPublications,
  getCategory,
  getCategoryPublications,
  getDefaultSites,
  getPublicationDetails: getPublication,
  getPublicationVersions,

  getChangedPublication,
  getChangedPublications,
  getUnpublishedSiteProduct,

  // UnpublishedPublications
  getUnpublishedPublication,
  getUnpublishedPublications,

  getReviews,
  getSite,
  getSitePolicies,
  getSitePublishers,
  getSiteFiles,
  getSiteModerators,
  searchLitePublication,
  searchLiteSites,
  searchPublications,
  searchAccounts,
  searchAuthors,
  searchSites,
  searchLiteAccounts,

  getAuthorFiles,

  getAuthorPerpetualSurveys,
  getAuthorPerpetualSurveyDetails,

  getAuthorReferendum,
  getAuthorReferendums,
  getAuthorReferendumComments,

  getModeratorDiscussion,
  getModeratorDiscussions,
  getModeratorDiscussionComments,
  getPublicationProposals,
  getModeratorUser,
  getProductDetails,
  getPublicationDetailsDiff,
  getModeratorProposals,
  getPublisherProposals,
  getReviewProposals,
  getUserProposals,
}

export const getApi = () => api
