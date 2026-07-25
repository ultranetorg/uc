import { OperationType } from "types"

const operationTypeToFairOperationTypeMap: Record<OperationType, string> = {
  "category-avatar-change": "CategoryAvatarChange",
  "category-creation": "CategoryCreation",
  "category-deletion": "CategoryDeletion",
  "category-movement": "CategoryMovement",
  "category-type-change": "CategoryTypeChange",
  "publication-creation": "PublicationCreation",
  "publication-deletion": "PublicationDeletion",
  "publication-publish": "PublicationPublish",
  "publication-updation": "PublicationUpdation",
  "publication-unpublish": "PublicationUnpublish",
  "review-creation": "ReviewCreation",
  "review-edit": "ReviewEdit",
  "review-status-change": "ReviewStatusChange",
  "store-authors-removal": "StoreAuthorsRemoval",
  "store-avatar-change": "StoreAvatarChange",
  "store-moderator-addition": "StoreModeratorAddition",
  "store-moderator-removal": "StoreModeratorRemoval",
  "store-name-change": "StoreNameChange",
  "store-info-updation": "StoreInfoUpdation",
  "user-registration": "UserRegistration",
  "user-unregistration": "UserUnregistration",
}

export const getFairOperationType = (type: OperationType): string | undefined =>
  operationTypeToFairOperationTypeMap[type]
