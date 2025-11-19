import { AccountBaseAvatar } from "./AccountBaseAvatar"
import { OperationType } from "./OperationType"
import { ProductType } from "./ProductType"

export type CreateProposalData = {
  title: string
  description?: string
  duration: string
  type?: OperationType
  options: CreateProposalDataOption[]
  categoryId?: string
  productId?: string
  publicationId?: string
  reviewId?: string
  userId?: string
}

export type CreateProposalDataOption = {
  title: string
  authorsIds?: string[]
  candidatesIds?: AccountBaseAvatar[]
  categoryTitle?: string
  description?: string
  fileId?: string
  moderatorsIds?: string[]
  nickname?: string
  parentCategoryId?: string
  siteTitle?: string
  slogan?: string
  type?: ProductType
}
