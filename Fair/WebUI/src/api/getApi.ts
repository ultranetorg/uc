import { Category, PaginationResponse, Publication, Site, SiteAuthor, SiteBase, SitePublication, User } from "types"

import { Api } from "./Api"
import { toPaginationResponse } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getCategory = (categoryId: string): Promise<Category> =>
  fetch(`${BASE_URL}/categories/${categoryId}`).then(res => res.json())

const getPublication = (publicationId: string): Promise<Publication> =>
  fetch(`${BASE_URL}/publications/${publicationId}`).then(res => res.json())

const getSite = (siteId: string): Promise<Site> => fetch(`${BASE_URL}/sites/${siteId}`).then(res => res.json())

const getSites = async (page?: number, pageSize?: number, name?: string): Promise<PaginationResponse<SiteBase>> => {
  const params = getUrlParams(name, page, pageSize)
  const res = await fetch(`${BASE_URL}/sites` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

const getSiteAuthor = (siteId: string, authorId: string): Promise<SiteAuthor> =>
  fetch(`${BASE_URL}/sites/${siteId}/authors/${authorId}`).then(res => res.json())

const getUrlParams = (name?: string, page?: number, pageSize?: number): URLSearchParams => {
  const params = new URLSearchParams()

  if (name !== null && name !== undefined && name != "") {
    params.append("name", name)
  }
  if (page !== undefined) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined) {
    params.append("pageSize", pageSize.toString())
  }

  return params
}

const getUser = (userId: string): Promise<User> => fetch(`${BASE_URL}/users/${userId}`).then(res => res.json())

const searchPublications = async (
  siteId: string,
  name?: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<SitePublication>> => {
  const params = getUrlParams(name, page, pageSize)
  const res = await fetch(`${BASE_URL}/sites/${siteId}/publications` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

const api: Api = {
  getCategory,
  getPublication,
  getSite,
  getSites,
  getSiteAuthor,
  getUser,
  searchPublications,
}

export const getApi = () => api
