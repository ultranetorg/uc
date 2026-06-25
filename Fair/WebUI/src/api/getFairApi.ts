import { DEFAULT_PAGE_SIZE_20 } from "config"
import { LIMIT_DEFAULT } from "constants/"
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
  ProductAuthor,
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

import { FairApi } from "./FairApi"
import { buildUrlParams, toTotalItemsResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getDefaultSites = (): Promise<SiteBase[]> => fetch(`${BASE_URL}/sites/default`).then(res => res.json())

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const getSitePolicies = (siteId: string): Promise<Policy[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/policies`).then(res => res.json())

const getSiteUsers = async (siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<User>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/users` + params)
  return await toTotalItemsResult(res)
}

const getSitePublishers = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Publisher>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publishers` + params)
  return await toTotalItemsResult(res)
}

const getSiteModerators = (siteId: string): Promise<Moderator[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/moderators`).then(res => res.json())

const getSiteFiles = async (siteId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/files` + params)
  if (!res.ok) throw new Error(`Failed to fetch site files`)
  return await toTotalItemsResult(res)
}

const searchSiteUsers = (siteId: string, query?: string, limit?: number): Promise<User[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/users/search?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res =>
    res.json(),
  )

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

const getUserAuthors = (userId: string): Promise<UserAuthors> =>
  fetch(`${BASE_URL}/users/${userId}/authors`).then(res => res.json())

const getUserDetails = (name: string): Promise<UserDetails> =>
  fetch(`${BASE_URL}/users/${name}/details`).then(res => res.json())

const getUserSiteExists = async (userId: string, siteId: string): Promise<boolean> => {
  const res = await fetch(`${BASE_URL}/users/${userId}/sites/${siteId}`, { method: "HEAD" })
  if (res.status === 404) return false
  if (res.ok) return true
  throw new Error(`Failed to check registration: ${res.status}`)
}

const getUserReviews = async (userId: string, page?: number): Promise<TotalItemsResult<Review>> => {
  const res = await fetch(`${BASE_URL}/users/${userId}/reviews` + (page && page > 0 ? `page=${page}` : ""))
  return await toTotalItemsResult(res)
}

const getAuthor = (authorId: string): Promise<AuthorDetails> =>
  fetch(`${BASE_URL}/authors/${authorId}`).then(res => res.json())

const getAuthorProducts = async (
  authorId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProductAuthor>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/authors/${authorId}/products` + params)
  return await toTotalItemsResult(res)
}

// Categories
const getCategoriesRoot = (siteId: string): Promise<CategoryBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/categories/root`).then(res => res.json())

const getCategoryDetails = (categoryId: string): Promise<Category> =>
  fetch(`${BASE_URL}/categories/${categoryId}`).then(res => res.json())

const getCategoriesTree = (siteId: string, depth?: number): Promise<CategoryParentBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/categories/tree` + (depth !== undefined ? `?depth=${depth}` : "")).then(res =>
    res.json(),
  )

const getCategoriesPublications = (siteId: string): Promise<CategoryPublications[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/categories/publications`).then(res => res.json())

const getPublication = (publicationId: string): Promise<PublicationDetails> =>
  fetch(`${BASE_URL}/publications/${publicationId}`).then(res => res.json())

const getPublicationVersions = (publicationId: string): Promise<PublicationVersionInfo> =>
  fetch(`${BASE_URL}/publications/${publicationId}/versions`).then(res => res.json())

const getChangedPublication = (siteId: string, changedPublicationId: string): Promise<PublicationChangedDetails> =>
  fetch(`${BASE_URL}/sites/${siteId}/publications/changed/${changedPublicationId}`).then(res => res.json())

const getChangedPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationChanged>> => {
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
): Promise<TotalItemsResult<PublicationUnpublished>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications/unpublished` + params)
  return await toTotalItemsResult(res)
}

const getPublisherPublications = async (
  siteId: string,
  publisherId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationAuthor>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publishers/${publisherId}/publications` + params)
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

const getProductStores = async (
  productId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProductStore>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/products/${productId}/stores` + params)
  return await toTotalItemsResult(res)
}

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

const getUserRegistrationProposals = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/proposals/user-registrations` + params)
  return await toTotalItemsResult(res)
}

const getUserUnregistrationProposals = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<UserUnregistrationProposal>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/sites/${siteId}/proposals/user-unregistrations` + params)
  return await toTotalItemsResult(res)
}

const api: FairApi = {
  getUser,
  getUserAuthors,
  getUserDetails,
  getUserSiteExists,
  getUserReviews,

  getPublisherPublications,
  getCategoriesTree,
  getCategoriesPublications,
  getCategoryDetails,
  getCategoriesRoot,
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
  getSiteUsers,
  getSitePublishers,
  getSiteFiles,
  getSiteModerators,
  searchSiteUsers,

  searchLitePublication,
  searchLiteSites,
  searchPublications,
  searchAccounts,
  searchAuthors,
  searchSites,
  searchLiteAccounts,

  getAuthor,
  getAuthorProducts,

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
  getProductStores,

  getPublicationDetailsDiff,
  getModeratorProposals,
  getPublisherProposals,
  getReviewProposals,

  // Proposals
  getUserRegistrationProposals,
  getUserUnregistrationProposals,
}

export const getFairApi = () => api
