import { CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES } from "constants/"
import { AccountBase, CreateProposalData, CreateProposalDataOption, OperationType, ProposalOption } from "types"
import { getFairOperationType } from "utils"

type FormOperationType = OperationType | "site-author-addition" | "site-author-removal"

const getOperationType = (type: FormOperationType): string => {
  if (type === "site-author-addition" || type === "site-author-removal") {
    return getFairOperationType("site-authors-change")!
  }
  return getFairOperationType(type as OperationType)!
}

const mapAccountsToIds = (accounts?: AccountBase[]): string[] => accounts?.map(account => account.id) ?? []

const mapOptionOperation = (type: FormOperationType, data: CreateProposalData, option: CreateProposalDataOption) => {
  switch (type) {
    // Category
    case "category-avatar-change":
      return { category: data.categoryId, file: option.fileId }
    case "category-creation":
      return { parent: data.categoryId, title: option.categoryTitle }
    case "category-deletion":
      return { category: data.categoryId }
    case "category-movement":
      return { category: data.categoryId !== null ? data.categoryId : undefined, parent: option.parentCategoryId }
    case "category-type-change":
      return { category: data.categoryId, type: option.type }

    // Publication
    case "publication-creation":
      return { productId: data.productId }
    case "publication-deletion":
      return { publicationId: data.publicationId }
    case "publication-publish":
      return { publicationId: data.publicationId, categoryId: option.categoryId }
    case "publication-updation":
      return { publicationId: data.publicationId, version: option.version }

    // Review
    case "review-status-change":
      return { reviewId: data.reviewId, status: option.status }

    // Site
    case "site-author-addition":
      return { additionsIds: mapAccountsToIds(option.candidatesAccounts), removalsIds: [] }
    case "site-author-removal":
      return { additionsIds: [], removalsIds: option.authorsIds }
    case "site-avatar-change":
      return { file: option.fileId }
    case "site-moderator-addition":
      return { candidatesIds: mapAccountsToIds(option.candidatesAccounts) }
    case "site-moderator-removal":
      return { moderatorId: option.moderatorsIds }
    case "site-nickname-change":
      return { name: option.name }
    case "site-text-change":
      return { title: option.siteTitle, slogan: option.slogan, description: option.description }

    // User
    case "user-deletion":
      return { userId: data.userId }

    default:
      return {}
  }
}

export const prepareProposalOptions = (data: CreateProposalData): ProposalOption[] => {
  const type = data.type! as FormOperationType
  const $type = getOperationType(type) as OperationType

  if (CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES.includes(type as OperationType)) {
    return [
      {
        title: "",
        operation: { $type, ...mapOptionOperation(type, data, data.options[0] ?? {}) },
      },
    ]
  }

  return data.options.map(option => ({
    title: option.title,
    operation: { $type, ...mapOptionOperation(type, data, option) },
  }))
}
