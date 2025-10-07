import { TFunction } from "i18next"

import { ApprovalPolicy, OperationClass, ProductType, ReviewStatus, Role } from "types"
import { CLEAR_ALL_VALUE } from "ui/components"

import { EditorOperationFields } from "./types"
import { validateUniqueCategoryTitle, validateUniqueCategoryType } from "./validations"

export const APPROVAL_POLICIES: ApprovalPolicy[] = [
  "none",
  "all-moderators",
  "any-moderator",
  "moderators-majority",
  "publishers-majority",
] as const

export const CATEGORY_TYPES: ProductType[] = ["none", "book", "game", "movie", "music", "software"] as const

export const OPERATION_CLASSES: OperationClass[] = [
  "fair-candidacy-declaration",
  "account-nickname-change",
  "account-avatar-change",
  "favorite-site-change",
  "author",
  "author-creation",
  "author-renewal",
  "author-moderation-reward",
  "author-owner-addition",
  "author-owner-removal",
  "author-nickname-change",
  "author-avatar-change",
  "author-text-change",
  "author-links-change",
  "product",
  "product-creation",
  "product-updation",
  "product-deletion",
  "site",
  "site-creation",
  "site-renewal",
  "site-policy-change",
  "site-authors-change",
  "site-moderators-change",
  "site-text-change",
  "site-avatar-change",
  "site-nickname-change",
  "user-registration",
  "user-deletion",
  "site-deletion",
  "category",
  "category-creation",
  "category-movement",
  "category-avatar-change",
  "category-type-change",
  "category-deletion",
  "publication",
  "publication-creation",
  "publication-remove-from-changed",
  "publication-publish",
  "publication-updation",
  "publication-deletion",
  "review",
  "review-creation",
  "review-status-change",
  "review-edit",
  "review-deletion",
  "proposal",
  "proposal-creation",
  "proposal-voting",
  "proposal-comment",
  "proposal-comment-creation",
  "proposal-comment-edit",
  "file",
  "file-creation",
  "file-deletion",
] as const

export const REVIEW_STATUSES: ReviewStatus[] = ["none", "accepted", "rejected"] as const

export const ROLES: (Role | string)[] = ["candidate", "moderator", "publisher", "user", CLEAR_ALL_VALUE] as const

export const getEditorOperationsFields = (t: TFunction): EditorOperationFields[] =>
  [
    {
      // Category
      operationType: "category-avatar-change",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      fields: [
        {
          valueType: "file",
          name: "fileId",
          placeholder: t("placeholders:selectFile"),
        },
      ],
    },
    {
      operationType: "category-creation",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      fields: [
        {
          valueType: "string",
          name: "categoryTitle",
          placeholder: t("placeholders:enterCategoryTitle"),
          rules: { required: t("validation:requiredCategoryTitle"), validate: validateUniqueCategoryTitle(t) },
        },
      ],
    },
    {
      operationType: "category-movement",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      fields: [
        {
          valueType: "category",
          name: "parentCategoryId",
          placeholder: t("placeholders:selectParentCategory"),
        },
      ],
    },
    {
      operationType: "category-type-change",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      fields: [
        {
          valueType: "category-type",
          name: "type",
          placeholder: t("placeholders:selectCategoryType"),
          rules: { required: true, validate: validateUniqueCategoryType(t) },
        },
      ],
    },
    {
      operationType: "category-deletion",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
    },

    // Publication
    {
      operationType: "publication-creation",
      parameterValueType: "product",
      parameterName: "productId",
      parameterLabel: t("common:product"),
      parameterPlaceholder: t("placeholders:selectProduct"),
    },
    {
      operationType: "publication-deletion",
      parameterValueType: "publication",
      parameterName: "publicationId",
      parameterLabel: t("common:publication"),
      parameterPlaceholder: t("placeholders:selectPublication"),
    },
    {
      operationType: "publication-publish",
      parameterValueType: "publication",
      parameterName: "publicationId",
      parameterLabel: t("common:publication"),
      parameterPlaceholder: t("placeholders:selectPublication"),
      fields: [
        {
          valueType: "category",
          name: "categoryId",
          placeholder: t("placeholders:selectCategory"),
        },
      ],
    },
    {
      operationType: "publication-remove-from-changed",
      parameterValueType: "publication",
      parameterName: "publicationId",
      parameterLabel: t("common:publication"),
      parameterPlaceholder: t("placeholders:selectPublication"),
    },
    {
      operationType: "publication-updation",
      parameterValueType: "publication",
      parameterName: "publicationId",
      parameterLabel: t("common:publication"),
      parameterPlaceholder: t("placeholders:selectPublication"),
      fields: [
        {
          valueType: "version",
          name: "version",
          placeholder: t("placeholders:selectVersion"),
        },
      ],
    },

    // Review
    {
      operationType: "review-status-change",
      parameterValueType: "review",
      parameterName: "reviewId",
      parameterLabel: t("common:review"),
      parameterPlaceholder: t("placeholders:selectReview"),
      fields: [
        {
          valueType: "review-status",
          name: "status",
          placeholder: t("placeholders:selectReviewStatus"),
        },
      ],
    },

    // Site
    {
      operationType: "site-authors-change",
      fields: [
        {
          valueType: "authors-array",
          name: "additions,removals",
          placeholder: t("selectAuthors"),
        },
      ],
    },
    {
      operationType: "site-avatar-change",
      fields: [
        {
          valueType: "file",
          name: "fileId",
          placeholder: t("placeholders:selectFile"),
        },
      ],
    },
    {
      operationType: "site-moderators-change",
      fields: [
        {
          valueType: "moderators-array",
          name: "additions,removals",
          placeholder: t("selectModerators"),
        },
      ],
    },
    {
      operationType: "site-nickname-change",
      fields: [
        {
          valueType: "string",
          name: "nickname",
          placeholder: t("placeholders:enterNickname"),
        },
      ],
    },
    {
      operationType: "site-policy-change",
      fields: [
        {
          valueType: "operation-class",
          name: "change",
          placeholder: t("placeholders:selectOperationClass"),
        },
        {
          valueType: "roles",
          name: "creators",
          placeholder: t("placeholders:selectRoles"),
        },
        {
          valueType: "approval-policy",
          name: "approval",
          placeholder: t("placeholders:selectApprovalPolicy"),
        },
      ],
    },
    {
      operationType: "site-text-change",
      fields: [
        {
          valueType: "string",
          name: "siteTitle",
          placeholder: t("placeholders:enterTitle"),
        },
        {
          valueType: "string",
          name: "slogan",
          placeholder: t("placeholders:enterSlogan"),
        },
        {
          valueType: "string-multiline",
          name: "description",
          placeholder: t("placeholders:enterDescription"),
        },
      ],
    },

    // User
    {
      operationType: "user-deletion",
      parameterValueType: "user",
      parameterName: "userId",
      parameterLabel: t("common:user"),
    },
  ] as const
