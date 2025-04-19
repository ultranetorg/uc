import { DEFAULT_PAGE_SIZE } from "constants"
import {
  Author,
  AuthorReferendum,
  AuthorReferendumDetails,
  Category,
  CategoryParentBase,
  ModeratorDispute,
  ModeratorDisputeDetails,
  ModeratorPublication,
  ModeratorReview,
  PaginationResponse,
  Publication,
  PublicationBase,
  Review,
  Site,
  SiteBase,
  User,
} from "types"

import { Api } from "./Api"
import { toPaginationResponse } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getUrlParams = (title?: string, page?: number, pageSize?: number): URLSearchParams => {
  const params = new URLSearchParams()

  if (title !== null && title !== undefined && title != "") {
    params.append("title", title)
  }
  if (page !== undefined) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined) {
    params.append("pageSize", pageSize.toString())
  }

  return params
}

const buildUrlParams = (page?: number, pageSize?: number, search?: string): URLSearchParams => {
  const params = new URLSearchParams()
  if (page !== undefined && page > 0) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined && pageSize !== DEFAULT_PAGE_SIZE) {
    params.append("pageSize", pageSize.toString())
  }
  if (!!search && search != "") {
    params.append("search", search)
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

const getAuthor = (authorId: string): Promise<Author> =>
  fetch(`${BASE_URL}/authors/${authorId}`).then(res => res.json())

const getCategories = async (
  siteId: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<CategoryParentBase>> => {
  const params = getPaginationParams(page, pageSize)
  const res = await fetch(`${BASE_URL}/sites/${siteId}/categories` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

const getCategory = (categoryId: string): Promise<Category> =>
  fetch(`${BASE_URL}/categories/${categoryId}`).then(res => res.json())

const getPublication = (publicationId: string): Promise<Publication> =>
  fetch(`${BASE_URL}/publications/${publicationId}`).then(res => res.json())

const getAuthorPublications = async (
  siteId: string,
  authorId: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<PublicationBase>> => {
  const params = getPaginationParams(page, pageSize)
  const res = await fetch(
    `${BASE_URL}/sites/${siteId}/authors/${authorId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const getCategoryPublications = async (
  categoryId: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<PublicationBase>> => {
  const params = getPaginationParams(page, pageSize)
  const res = await fetch(
    `${BASE_URL}/categories/${categoryId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""),
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

const getSites = async (page?: number, pageSize?: number, search?: string): Promise<PaginationResponse<SiteBase>> => {
  const params = buildUrlParams(page, pageSize, search)
  const res = await fetch(`${BASE_URL}/sites` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

const getUser = (userId: string): Promise<User> => fetch(`${BASE_URL}/users/${userId}`).then(res => res.json())

const searchPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  title?: string,
): Promise<PaginationResponse<Publication>> => {
  const params = getUrlParams(title, page, pageSize)
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

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
  getAuthor,
  getCategories,
  getCategory,
  getPublication,
  getAuthorPublications,
  getReviews,
  getSite,
  getSites,
  getUser,
  searchPublications,
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
