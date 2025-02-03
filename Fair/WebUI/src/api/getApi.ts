import { Author, Category, PaginationResponse, Publication, Site, SitePublication, User } from "types"

import { Api } from "./Api"
import { toPaginationResponse } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL, VITE_APP_SITE_ID: SITE_ID } = import.meta.env

const getAuthor = (authorId: string): Promise<Author> =>
  fetch(`${BASE_URL}/authors/${authorId}`).then(res => res.json())

const getCategory = (categoryId: string): Promise<Category> =>
  fetch(`${BASE_URL}/categories/${categoryId}`).then(res => res.json())

const getPublication = (publicationId: string): Promise<Publication> =>
  fetch(`${BASE_URL}/publications/${publicationId}`).then(res => res.json())

const getSite = (): Promise<Site> => fetch(`${BASE_URL}/sites/${SITE_ID}`).then(res => res.json())

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
  name?: string,
  page?: number,
  pageSize?: number,
): Promise<PaginationResponse<SitePublication>> => {
  const params = getUrlParams(name, page, pageSize)
  const res = await fetch(
    `${BASE_URL}/sites/${SITE_ID}/publications` + (params.size > 0 ? `?${params.toString()}` : ""),
  )
  return await toPaginationResponse(res)
}

const api: Api = {
  getAuthor,
  getCategory,
  getPublication,
  getSite,
  getUser,
  searchPublications,
}

export const getApi = () => api
