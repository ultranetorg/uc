import { DEFAULT_PAGE_SIZE_2 } from "constants"
import {
  Author,
  AuthorReferendum,
  AuthorReferendumDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  ModeratorDispute,
  ModeratorDisputeDetails,
  ModeratorPublication,
  ModeratorReview,
  PaginationResponse,
  PaginationResult,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  Review,
  Site,
  SiteBase,
  SiteLightSearch,
  User,
} from "types"

import { Api } from "./Api"
import { toPaginationResponse, toPaginationResult } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

// TODO: refactor these methods: getUrlParams, buildUrlParams, buildUrlParams2, getPaginationParams.
const buildUrlParams = (page?: number, pageSize?: number, search?: string): URLSearchParams => {
  const params = new URLSearchParams()
  if (page !== undefined && page > 0) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined && pageSize !== DEFAULT_PAGE_SIZE_2) {
    params.append("pageSize", pageSize.toString())
  }
  if (!!search && search != "") {
    params.append("search", search)
  }
  return params
}

const buildUrlParams2 = (query?: string, page?: number): URLSearchParams => {
  const params = new URLSearchParams()
  if (!!query && query != "") {
    params.append("query", query)
  }
  if (page !== undefined && page > 0) {
    params.append("page", page.toString())
  }
  return params
}

const getPaginationParams = (page?: number, pageSize?: number): URLSearchParams => {
  const params = new URLSearchParams()

  if (page !== undefined) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined) {
    params.append("pageSize", pageSize.toString())
  }

  return params
}

const getDefaultSites = (): Promise<SiteBase[]> => fetch(`${BASE_URL}/sites/default`).then(res => res.json())

const getAuthor = (authorId: string): Promise<Author> =>
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

const getAuthorPublications = async (
  siteId: string,
  authorId: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<PublicationAuthor>> => {
  const params = getPaginationParams(page, pageSize)
  const res = await fetch(
    `${BASE_URL}/sites/${siteId}/authors/${authorId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getCategoryPublications = async (categoryId: string, page?: number): Promise<PaginationResponse<Publication>> => {
  const res = await fetch(
    `${BASE_URL}/categories/${categoryId}/publications${page && page > 0 ? "?page=" + page.toString() : ""}`,
  )
  return await toPaginationResponse(res)
}

const getReviews = async (
  publicationId: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<Review>> => {
  const params = getPaginationParams(page, pageSize)
  const res = await fetch(
    `${BASE_URL}/publications/${publicationId}/reviews` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const searchSites = async (query?: string, page?: number): Promise<PaginationResult<SiteBase>> => {
  const params = buildUrlParams2(query, page)
  const res = await fetch(`${BASE_URL}/sites` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResult(res)
}

const searchLightSites = async (query?: string): Promise<SiteLightSearch[]> =>
  fetch(`${BASE_URL}/sites/search?query=${query}`).then(res => res.json())

const getUser = (userId: string): Promise<User> => fetch(`${BASE_URL}/users/${userId}`).then(res => res.json())

const searchPublications = async (
  siteId: string,
  query?: string,
  page?: number,
): Promise<PaginationResult<PublicationExtended>> => {
  const params = buildUrlParams2(query, page)
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResult(res)
}

const searchLightPublication = (siteId: string, query?: string): Promise<PublicationBase[]> =>
  fetch(`${BASE_URL}/sites/${siteId}/publications/search?query=${query}`).then(res => res.json())

const getAuthorReferendum = (siteId: string, referendumId: string): Promise<AuthorReferendumDetails> =>
  fetch(`${BASE_URL}/author/sites/${siteId}/referendums/${referendumId}`).then(res => res.json())

const getAuthorReferendums = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<PaginationResponse<AuthorReferendum>> => {
  const params = buildUrlParams(page, pageSize, search)
  const res = await fetch(
    `${BASE_URL}/author/sites/${siteId}/referendums` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getModeratorDispute = async (siteId: string, disputeId: string): Promise<ModeratorDisputeDetails> =>
  fetch(`${BASE_URL}/moderator/sites/${siteId}/disputes/${disputeId}`).then(res => res.json())

const getModeratorDisputes = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<PaginationResponse<ModeratorDispute>> => {
  const params = buildUrlParams(page, pageSize, search)
  const res = await fetch(
    `${BASE_URL}/moderator/sites/${siteId}/disputes` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getModeratorPublication = (publicationId: string): Promise<ModeratorPublication> =>
  fetch(`${BASE_URL}/moderator/publications/${publicationId}`).then(res => res.json())

const getModeratorPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<PaginationResponse<ModeratorPublication>> => {
  const params = buildUrlParams(page, pageSize, search)
  const res = await fetch(
    `${BASE_URL}/moderator/sites/${siteId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getModeratorReview = (reviewId: string): Promise<ModeratorReview> =>
  fetch(`${BASE_URL}/moderator/reviews/${reviewId}`).then(res => res.json())

const getModeratorReviews = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<PaginationResponse<ModeratorReview>> => {
  const params = buildUrlParams(page, pageSize, search)
  const res = await fetch(
    `${BASE_URL}/moderator/sites/${siteId}/reviews` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const api: Api = {
  getDefaultSites,
  getAuthor,
  getCategories,
  getCategory,
  getCategoriesPublications,
  getPublication,
  getAuthorPublications,
  getReviews,
  getSite,
  searchSites,
  searchLightSites,
  getUser,
  searchPublications,
  searchLightPublication,
  getAuthorReferendum,
  getCategoryPublications,
  getAuthorReferendums,
  getModeratorDispute,
  getModeratorDisputes,
  getModeratorPublication,
  getModeratorPublications,
  getModeratorReview,
  getModeratorReviews,
}

export const getApi = () => api
