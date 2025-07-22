import { DEFAULT_PAGE_SIZE_2 } from "config"
import {
  AuthorDetails,
  AuthorReferendum,
  AuthorReferendumDetails,
  Category,
  CategoryParentBase,
  CategoryPublications,
  ModeratorDiscussion,
  ModeratorDiscussionComment,
  ModeratorDiscussionDetails,
  ModeratorPublication,
  ModeratorReview,
  Publication,
  PublicationAuthor,
  PublicationBase,
  PublicationDetails,
  PublicationExtended,
  Review,
  Site,
  SiteBase,
  SiteLiteSearch,
  TotalItemsResult,
  User,
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

const getAuthorReferendum = (siteId: string, referendumId: string): Promise<AuthorReferendumDetails> =>
  fetch(`${BASE_URL}/author/sites/${siteId}/referendums/${referendumId}`).then(res => res.json())

const getAuthorReferendums = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<AuthorReferendum>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_2, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/author/sites/${siteId}/referendums` + params)
  return await toTotalItemsResult(res)
}

const getModeratorDiscussion = async (siteId: string, disputeId: string): Promise<ModeratorDiscussionDetails> =>
  fetch(`${BASE_URL}/moderator/sites/${siteId}/disputes/${disputeId}`).then(res => res.json())

const getModeratorDiscussionComments = async (
  siteId: string,
  disputeId: string,
  page?: number,
  pageSize?: number,
): Promise<TotalItemsResult<ModeratorDiscussionComment>> => {
  const params = buildUrlParams(
    { page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_2, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/disputes/${disputeId}/comments` + params)
  return await toTotalItemsResult(res)
}

const getModeratorDiscussions = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<ModeratorDiscussion>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_2, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/disputes` + params)
  return await toTotalItemsResult(res)
}

const getModeratorPublication = (publicationId: string): Promise<ModeratorPublication> =>
  fetch(`${BASE_URL}/moderator/publications/${publicationId}`).then(res => res.json())

const getModeratorPublications = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<ModeratorPublication>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_2, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/publications` + params)
  return await toTotalItemsResult(res)
}

const getModeratorReview = (reviewId: string): Promise<ModeratorReview> =>
  fetch(`${BASE_URL}/moderator/reviews/${reviewId}`).then(res => res.json())

const getModeratorReviews = async (
  siteId: string,
  page?: number,
  pageSize?: number,
  search?: string,
): Promise<TotalItemsResult<ModeratorReview>> => {
  const params = buildUrlParams(
    { search, page, pageSize },
    { pageSize: x => x !== DEFAULT_PAGE_SIZE_2, page: x => !!x && x > 0 },
  )
  const res = await fetch(`${BASE_URL}/moderator/sites/${siteId}/reviews` + params)
  return await toTotalItemsResult(res)
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
  searchLiteSites,
  getUser,
  searchPublications,
  searchLitePublication,
  getAuthorReferendum,
  getCategoryPublications,
  getAuthorReferendums,
  getModeratorDiscussion,
  getModeratorDiscussionComments,
  getModeratorDiscussions,
  getModeratorPublication,
  getModeratorPublications,
  getModeratorReview,
  getModeratorReviews,
}

export const getApi = () => api
