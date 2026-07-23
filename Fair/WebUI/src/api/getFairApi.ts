import { DEFAULT_PAGE_SIZE_20 } from "config"
import { LIMIT_DEFAULT } from "constants/"
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
  UserSearchLite,
  UserUnregistrationProposal,
} from "types"

import { FairApi } from "./FairApi"
import { buildUrlParams, fetchApi, toTotalItemsResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getDefaultStores = (): Promise<StoreBase[]> => fetch(`${BASE_URL}/stores/default`).then(res => res.json())

const getStore = (storeId: string): Promise<Store> => fetch(`${BASE_URL}/stores/${storeId}`).then(res => res.json())

const getStorePolicies = (storeId: string): Promise<Policy[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/policies`).then(res => res.json())

const getStoreUsers = async (storeId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<User>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/users` + params)
  return await toTotalItemsResult(res)
}

const getStorePublishers = async (
  storeId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Publisher>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/publishers` + params)
  return await toTotalItemsResult(res)
}

const getStoreModerators = (storeId: string): Promise<Moderator[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/moderators`).then(res => res.json())

const getStoreFiles = async (storeId: string, page?: number, pageSize?: number): Promise<TotalItemsResult<File>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/files` + params)
  if (!res.ok) throw new Error(`Failed to fetch store files`)
  return await toTotalItemsResult(res)
}

const searchStoreUsers = (storeId: string, query?: string, limit?: number): Promise<User[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/users/search?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res =>
    res.json(),
  )

const searchAccounts = (query?: string, limit?: number): Promise<UserBase[]> =>
  fetch(`${BASE_URL}/accounts?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res => res.json())

const searchAuthors = (query?: string, limit?: number): Promise<AuthorBaseAvatar[]> =>
  fetch(`${BASE_URL}/authors?query=${query}&limit=${limit ?? LIMIT_DEFAULT}`).then(res => res.json())

const searchStores = async (query?: string, page?: number): Promise<TotalItemsResult<StoreBase>> => {
  const params = buildUrlParams({ query, page })
  const res = await fetch(`${BASE_URL}/stores` + params)
  return await toTotalItemsResult(res)
}

const searchLiteStores = (query?: string): Promise<StoreLiteSearch[]> =>
  fetch(`${BASE_URL}/stores/search?query=${query}`).then(res => res.json())

const searchPublications = async (storeId: string, query?: string, page?: number): Promise<PublicationExtended[]> => {
  const params = buildUrlParams({ query, page })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/publications` + params)
  return res.json()
}

const searchLitePublication = (storeId: string, query?: string): Promise<PublicationBase[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/publications/search?query=${query}`).then(res => res.json())

const searchLiteAccounts = (query?: string): Promise<UserSearchLite[]> =>
  fetch(`${BASE_URL}/accounts/search?query=${query}`).then(res => res.json())

const searchLiteProducts = (query: string): Promise<ProductSearchResultBase[]> =>
  fetch(`${BASE_URL}/products/search?query=${query}`).then(res => res.json())

const searchProducts = async (query?: string, page?: number, pageSize?: number): Promise<ProductSearchResult[]> => {
  const params = buildUrlParams({ query, page, pageSize })
  const res = await fetch(`${BASE_URL}/products` + params)
  return res.json()
}

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

const getUserAuthors = (userId: string): Promise<UserAuthors> => fetchApi(fetch(`${BASE_URL}/users/${userId}/authors`))

const getUserDetails = (name: string): Promise<UserDetails> =>
  fetch(`${BASE_URL}/users/${name}/details`).then(res => res.json())

const getUserStoreExists = async (userId: string, storeId: string): Promise<boolean> => {
  const res = await fetch(`${BASE_URL}/users/${userId}/stores/${storeId}`, { method: "HEAD" })
  if (res.status === 404) return false
  if (res.ok) return true
  throw new Error(`Failed to check registration: ${res.status}`)
}

const getUserReviews = async (userId: string, page?: number): Promise<TotalItemsResult<Review>> => {
  const res = await fetch(`${BASE_URL}/users/${userId}/reviews` + (page && page > 0 ? `page=${page}` : ""))
  return await toTotalItemsResult(res)
}

const getAuthor = (authorId: string): Promise<AuthorDetails> =>
  fetchApi<AuthorDetails>(fetch(`${BASE_URL}/authors/${authorId}`))

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
const getCategoriesRoot = (storeId: string): Promise<CategoryBase[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/categories/root`).then(res => res.json())

const getCategoryDetails = (categoryId: string): Promise<Category> =>
  fetchApi(fetch(`${BASE_URL}/categories/${categoryId}`))

const getCategoriesTree = (storeId: string, depth?: number): Promise<CategoryParentBase[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/categories/tree` + (depth !== undefined ? `?depth=${depth}` : "")).then(res =>
    res.json(),
  )

const getCategoriesPublications = (storeId: string): Promise<CategoryPublications[]> =>
  fetch(`${BASE_URL}/stores/${storeId}/categories/publications`).then(res => res.json())

const getPublication = (publicationId: string): Promise<PublicationDetails> =>
  fetchApi(fetch(`${BASE_URL}/publications/${publicationId}`))

const getPublicationVersions = (publicationId: string): Promise<PublicationVersionInfo> =>
  fetch(`${BASE_URL}/publications/${publicationId}/versions`).then(res => res.json())

const getChangedPublication = (storeId: string, changedPublicationId: string): Promise<PublicationChangedDetails> =>
  fetch(`${BASE_URL}/stores/${storeId}/publications/changed/${changedPublicationId}`).then(res => res.json())

const getChangedPublications = async (
  storeId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationChanged>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/publications/changed` + params)
  return await toTotalItemsResult(res)
}

const getUnpublishedStoreProduct = async (storeId: string, unpublishedProductId: string): Promise<ProductDetails> => {
  const res = await fetch(`${BASE_URL}/stores/${storeId}/products/unpublished/${unpublishedProductId}`)
  if (!res.ok) {
    throw new Error()
  }

  return await res.json()
}

const getUnpublishedPublication = (storeId: string, publicationId: string): Promise<ProductDetails> =>
  fetch(`${BASE_URL}/stores/${storeId}/publications/unpublished/${publicationId}`).then(res => res.json())

const getUnpublishedPublications = async (
  storeId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationUnpublished>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/publications/unpublished` + params)
  return await toTotalItemsResult(res)
}

const getPublisherPublications = async (
  storeId: string,
  publisherId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublicationAuthor>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/publishers/${publisherId}/publications` + params)
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
  storeId: string,
  authorId?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<File>> => {
  const params = buildUrlParams({ page, pageSize })
  const res = await fetch(`${BASE_URL}/stores/${storeId}/authors/${authorId}/files` + params)
  if (!res.ok) throw new Error(`Failed to fetch author files`)
  return await toTotalItemsResult(res)
}

const getAuthorPerpetualSurveys = (storeId: string): Promise<PerpetualSurvey[]> =>
  fetch(`${BASE_URL}/author/stores/${storeId}/perpetual-surveys`).then(res => res.json())

const getAuthorPerpetualSurveyDetails = (storeId: string, perpetualSurveyId: string): Promise<PerpetualSurveyDetails> =>
  fetch(`${BASE_URL}/author/stores/${storeId}/perpetual-surveys/${perpetualSurveyId}`).then(res => res.json())

const getAuthorReferendum = (storeId: string, referendumId: string): Promise<ProposalDetails> =>
  fetch(`${BASE_URL}/author/stores/${storeId}/referendums/${referendumId}`).then(res => res.json())

const getAuthorReferendumComments = async (
  storeId: string,
  referendumId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProposalComment>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/author/stores/${storeId}/referendums/${referendumId}/comments` + params)
  return await toTotalItemsResult(res)
}

const getAuthorReferendums = async (
  storeId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/author/stores/${storeId}/referendums` + params)
  return await toTotalItemsResult(res)
}

const getModeratorDiscussion = async (storeId: string, discussionId: string): Promise<ProposalDetails> => {
  const res = await fetch(`${BASE_URL}/moderator/stores/${storeId}/discussions/${discussionId}`)
  if (!res.ok) {
    throw new Error()
  }

  return await res.json()
}

const getModeratorDiscussionComments = async (
  storeId: string,
  discussionId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ProposalComment>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/stores/${storeId}/discussions/${discussionId}/comments` + params)
  return await toTotalItemsResult(res)
}

const getModeratorDiscussions = async (
  storeId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/stores/${storeId}/discussions` + params)
  return await toTotalItemsResult(res)
}

const getPublicationProposals = async (
  storeId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<PublicationProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/stores/${storeId}/publications` + params)
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
  storeId: string,
  search?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ModeratorProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/proposals/moderators` + params)
  return await toTotalItemsResult(res)
}

const getPublisherProposals = async (
  storeId: string,
  search?: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<PublisherProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/proposals/publishers` + params)
  return await toTotalItemsResult(res)
}

const getReviewProposals = async (
  storeId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<ReviewProposal>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/stores/${storeId}/reviews` + params)
  return await toTotalItemsResult(res)
}

const getUserRegistrationProposals = async (
  storeId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<Proposal>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/proposals/user-registrations` + params)
  return await toTotalItemsResult(res)
}

const getUserUnregistrationProposals = async (
  storeId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<UserUnregistrationProposal>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_20, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/stores/${storeId}/proposals/user-unregistrations` + params)
  return await toTotalItemsResult(res)
}

const api: FairApi = {
  getUser,
  getUserAuthors,
  getUserDetails,
  getUserStoreExists,
  getUserReviews,

  getPublisherPublications,
  getCategoriesTree,
  getCategoriesPublications,
  getCategoryDetails,
  getCategoriesRoot,
  getCategoryPublications,
  getDefaultStores,
  getPublicationDetails: getPublication,
  getPublicationVersions,

  getChangedPublication,
  getChangedPublications,
  getUnpublishedStoreProduct,

  // UnpublishedPublications
  getUnpublishedPublication,
  getUnpublishedPublications,

  getReviews,
  getStore,
  getStorePolicies,
  getStoreUsers,
  getStorePublishers,
  getStoreFiles,
  getStoreModerators,
  searchStoreUsers,

  searchLitePublication,
  searchLiteStores,
  searchPublications,
  searchAccounts,
  searchAuthors,
  searchStores,
  searchLiteAccounts,
  searchLiteProducts,
  searchProducts,

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
