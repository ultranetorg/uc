import { AccountBase } from "./AccountBase"
import { AuthorBaseAvatar } from "./AuthorBaseAvatar"
import { OperationType } from "./OperationType"
import { ProductType } from "./ProductType"

export type CreateProposalData = {
  title: string
  description?: string
  type?: OperationType
  options: CreateProposalDataOption[]
  categoryId?: string
  moderatorId?: string
  productId?: string
  publicationId?: string
  publisherId?: string
  reviewId?: string
  userId?: string
  previousPath?: string
}

export type CreateProposalDataOption = {
  title: string
  authors?: AuthorBaseAvatar[]
  categoryTitle?: string
  description?: string
  fileId?: string
  moderators?: AccountBase[]
  name?: string
  parentCategoryId?: string
  siteTitle?: string
  slogan?: string
  type?: ProductType
}
