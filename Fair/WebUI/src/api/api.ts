import {
  Account,
  Auctions,
  AuctionBid,
  AuctionDetails,
  Author,
  Chain,
  Operation,
  PaginatedRequest,
  PaginatedResponse,
  Resource,
  ResourceInfo,
  Round,
  RoundTransaction,
  Search,
  Transaction,
  ResourceLink,
} from "types"

type ChainMethods = {
  get(): Promise<Chain>
}

type AccountsMethods = {
  getByAddress(address: string): Promise<Account>
  getOperations(address: string, page: number, pageSize?: number): Promise<PaginatedResponse<Operation>>
}

type AuthorsMethods = {
  getByName(name: string): Promise<Author>
  getResources(name: string, page: number, pageSize?: number): Promise<PaginatedResponse<ResourceInfo>>
}

type ResourcesMethods = {
  getByAuthorName(author: string, name: string): Promise<Resource>
  getLinks(author: string, resource: string, page: number, pageSize?: number): Promise<PaginatedResponse<ResourceLink>>
  getInboundLinks(
    author: string,
    resource: string,
    page: number,
    pageSize?: number,
  ): Promise<PaginatedResponse<ResourceLink>>
}

type RoundsMethods = {
  getById(id: number | string): Promise<Round>
  getTransactions(id: number | string, page: number, pageSize?: number): Promise<PaginatedResponse<RoundTransaction>>
}

type TransactionsMethods = {
  getById(id: string): Promise<Transaction>
  getOperations(id: string, page: number, pageSize?: number): Promise<PaginatedResponse<Operation>>
}

type OperationsMethods = {
  getById(id: number | string): Promise<Operation>
}

type AuctionsMethods = {
  getByName(name: string): Promise<AuctionDetails>
  getAll(request?: PaginatedRequest): Promise<Auctions>
  getBids(name: string, page: number, pageSize?: number): Promise<PaginatedResponse<AuctionBid>>
}

type SearchMethods = {
  search(query: string, pagination: PaginatedRequest): Promise<Search[]>
}

export type Api = {
  chain: ChainMethods
  accounts: AccountsMethods
  authors: AuthorsMethods
  resources: ResourcesMethods
  rounds: RoundsMethods
  transactions: TransactionsMethods
  operations: OperationsMethods
  auctions: AuctionsMethods
} & SearchMethods
