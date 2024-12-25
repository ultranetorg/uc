import axios, { AxiosError } from "axios"

import { DEFAULT_PAGE_SIZE } from "constants"
import {
  Account,
  AuctionBid,
  AuctionDetails,
  Auctions,
  Author,
  Chain,
  Operation,
  PaginatedRequest,
  PaginatedResponse,
  Resource,
  ResourceInfo,
  ResourceLink,
  Round,
  RoundTransaction,
  Search,
  Transaction,
} from "types"
import { combineUrl, fromAxiosResponse, fromAxiosError } from "utils"

import { Api } from "./api"
import { interceptDates } from "./interceptors"
import { toAuctions, toPaginatedResponse } from "./utils"

const { VITE_APP_API_BASE_URL } = import.meta.env

// TODO: check env variable.

const instance = axios.create({
  baseURL: combineUrl(VITE_APP_API_BASE_URL!, "api"),
})

instance.interceptors.response.use(
  response => {
    interceptDates(response.data)
    return response
  },
  (error: AxiosError<{ message: string; errorCode: number; stackTrace: string }>) => {
    const pathname = window.location.pathname
    const err = !!error.response ? fromAxiosResponse(error.response, pathname) : fromAxiosError(error, pathname)
    return Promise.reject(err)
  },
)

async function getDashboardData(): Promise<Chain> {
  const response = await instance.get("chain")
  return response.data
}

async function getAccountByAddress(address: string): Promise<Account> {
  const response = await instance.get(`accounts/${address}`)
  return response.data
}

const getAccountOperations = async (
  address: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<Operation>> => {
  const response = await instance.get(
    `accounts/${address}/operations?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function getAuthorByName(name: string): Promise<Author> {
  const response = await instance.get<Author>(`authors/${name}`)
  return response.data
}

const getAuthorResources = async (
  name: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<ResourceInfo>> => {
  const response = await instance.get(
    `authors/${name}/resources?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function getResourceByAuthorName(author: string, name: string): Promise<Resource> {
  const response = await instance.get<Resource>(`authors/${author}/resources/${name}`)
  return response.data
}

const getResourceLinks = async (
  author: string,
  resource: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<ResourceLink>> => {
  const response = await instance.get(
    `${author}/resources/${resource}/links?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

const getResourceInboundLinks = async (
  author: string,
  resource: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<ResourceLink>> => {
  const response = await instance.get(
    `${author}/resources/${resource}/inbound_links?page=${page}${
      !!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE
    }`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function getRoundById(id: number | string): Promise<Round> {
  const response = await instance.get(`rounds/${id}`)
  return response.data
}

const getRoundTransactions = async (
  id: number | string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<RoundTransaction>> => {
  const response = await instance.get(
    `rounds/${id}/transactions?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function getTransactionById(id: string): Promise<Transaction> {
  const response = await instance.get(`transactions/${id}`)
  return response.data
}

const getTransactionOperations = async (
  id: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<Operation>> => {
  const response = await instance.get(
    `transactions/${id}/operations?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function getOperationById(id: number | string): Promise<Operation> {
  const response = await instance.get(`operations/${id}`)
  return response.data
}

async function getAuctionByName(name: string): Promise<AuctionDetails> {
  const response = await instance.get(`auctions/${name}`)
  return response.data
}

async function getAllAuctionsAsync(request?: PaginatedRequest): Promise<Auctions> {
  const response = await instance.get(`auctions`, { params: request })
  return toAuctions(response.data, response.headers)
}

const getAuctionBids = async (
  name: string,
  page: number,
  pageSize?: number,
): Promise<PaginatedResponse<AuctionBid>> => {
  const response = await instance.get(
    `auctions/${name}/bids?page=${page}${!!pageSize ? "&pageSize=" + pageSize : DEFAULT_PAGE_SIZE}`,
  )
  return toPaginatedResponse(response.data, response.headers)
}

async function search<T extends Search>(query: string, pagination: PaginatedRequest): Promise<T[]> {
  const response = await instance.get<T[]>(`search`, { params: { query, ...pagination } })
  return response.data
}

const api: Api = {
  chain: {
    get: getDashboardData,
  },
  accounts: {
    getByAddress: getAccountByAddress,
    getOperations: getAccountOperations,
  },
  authors: {
    getByName: getAuthorByName,
    getResources: getAuthorResources,
  },
  resources: {
    getByAuthorName: getResourceByAuthorName,
    getLinks: getResourceLinks,
    getInboundLinks: getResourceInboundLinks,
  },
  rounds: {
    getById: getRoundById,
    getTransactions: getRoundTransactions,
  },
  transactions: {
    getById: getTransactionById,
    getOperations: getTransactionOperations,
  },
  operations: {
    getById: getOperationById,
  },
  auctions: {
    getByName: getAuctionByName,
    getAll: getAllAuctionsAsync,
    getBids: getAuctionBids,
  },
  search,
}

export const getApi = (): Api => api
