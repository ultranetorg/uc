import { CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES } from "constants/"
import {
  UserBase,
  AuthorBaseAvatar,
  CreateProposalData,
  CreateProposalDataOption,
  OperationType,
  ProposalOption,
} from "types"
import { getFairOperationType } from "utils"

const mapAuthorsToIds = (accounts?: AuthorBaseAvatar[]): string[] => accounts?.map(x => x.id) ?? []

const mapUsersToIds = (accounts?: UserBase[]): string[] => accounts?.map(x => x.id) ?? []

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
      return { product: data.productId }
    case "publication-deletion":
      return { publication: data.publicationId }
    case "publication-publish":
      // @ts-expect-error fix
      return { publication: data.publicationId, category: option.categoryId! }
    case "publication-updation":
      // @ts-expect-error fix
      return { publication: data.publicationId, version: option.version }
    case "publication-unpublish":
      return { publication: data.publicationId }

    // Review
    case "review-status-change":
      // @ts-expect-error fix
      return { review: data.reviewId, status: option.status }

    // Store
    case "store-authors-removal":
      return { authors: mapAuthorsToIds(option.authors) }
    case "store-avatar-change":
      return { file: option.fileId }
    case "store-moderator-addition":
      return { candidates: mapUsersToIds(option.moderators) }
    case "store-moderator-removal":
      return { moderator: mapUsersToIds(option.moderators)[0] }
    case "store-name-change":
      return { name: option.name }
    case "store-info-updation":
      return { title: option.storeTitle, slogan: option.slogan, description: option.description }

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
