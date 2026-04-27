import { CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES } from "constants/"
import {
  AccountBase,
  AuthorBaseAvatar,
  CreateProposalData,
  CreateProposalDataOption,
  OperationType,
  Policy,
  ProposalOption,
  ProposalType,
  Site,
} from "types"
import { getFairOperationType } from "utils"

const mapAuthorsToIds = (accounts?: AuthorBaseAvatar[]): string[] => accounts?.map(x => x.id) ?? []

const mapAccountsToIds = (accounts?: AccountBase[]): string[] => accounts?.map(x => x.id) ?? []

const mapOptionOperation = (type: OperationType, data: CreateProposalData, option: CreateProposalDataOption) => {
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
      return { publication: data.publicationId, category: option.categoryId! }
    case "publication-updation":
      return { publicationId: data.publicationId, version: option.version }

    // Review
    case "review-status-change":
      return { reviewId: data.reviewId, status: option.status }

    // Site
    case "site-authors-removal":
      return { authors: mapAuthorsToIds(option.authors) }
    case "site-avatar-change":
      return { file: option.fileId }
    case "site-moderator-addition":
      return { candidates: mapAccountsToIds(option.moderators) }
    case "site-moderator-removal":
      return { moderator: mapAccountsToIds(option.moderators)[0] }
    case "site-name-change":
      return { name: option.name }
    case "site-text-change":
      return { title: option.siteTitle, slogan: option.slogan, description: option.description }

    // User
    case "user-unregistration":
      return { user: data.userId }

    default:
      return {}
  }
}

export const prepareProposalOptions = (data: CreateProposalData): ProposalOption[] => {
  const type = data.type! as OperationType
  const $type = getFairOperationType(type)

  if (CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES.includes(type as OperationType)) {
    return [
      {
        title: "",
        // @ts-expect-error fix
        operation: { $type, ...mapOptionOperation(type, data, data.options[0] ?? {}) },
      },
    ]
  }

  // @ts-expect-error fix
  return data.options.map(option => ({
    title: option.title,
    operation: { $type, ...mapOptionOperation(type, data, option) },
  }))
}

export const isVotingRequired = (
  proposalType: ProposalType,
  site?: Site,
  operation?: OperationType,
  policies?: Policy[],
): boolean => {
  if (!site || !operation || !policies) return true

  const votersCount = proposalType === "discussion" ? site.moderatorsIds.length : site.authorsIds.length
  const policy = policies.find(x => x.operationClass === operation)

  if (policy) {
    switch (policy.approval) {
      case "any-moderator":
        return false
      case "moderators-majority":
      case "publishers-majority":
        return votersCount > 2
      case "all-moderators":
        return votersCount > 1
    }
  }

  return true
}
