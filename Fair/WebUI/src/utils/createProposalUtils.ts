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
  "site-authors-removal": "SiteAuthorsRemoval",
  "site-avatar-change": "SiteAvatarChange",
  "site-moderator-addition": "SiteModeratorAddition",
  "site-moderator-removal": "SiteModeratorRemoval",
  "site-name-change": "SiteNameChange",
  "site-text-change": "SiteTextChange",
  "user-registration": "UserRegistration",
  "user-unregistration": "UserUnregistration",
}

export const getFairOperationType = (type: OperationType): string | undefined =>
  operationTypeToFairOperationTypeMap[type]
