import { TFunction } from "i18next"

import { ProductType, ReviewStatus } from "types"

import { EditorOperationFields } from "./types"
import {
  validateUniqueCategoryTitle,
  validateUniqueCategoryType,
  validateUniqueParentCategory,
  validateUniqueSiteNickname,
} from "./validations"

export const CATEGORY_TYPES: ProductType[] = ["book", "game", "movie", "music", "software"] as const

export const REVIEW_STATUSES: ReviewStatus[] = ["none", "accepted", "rejected"] as const

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
          // @ts-expect-error incompatible param.
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
      // @ts-expect-error incompatible param.
      parameterRules: { validate: validateUniqueParentCategory(t) },
      fields: [
        {
          valueType: "category",
          name: "parentCategoryId",
          placeholder: t("placeholders:selectParentCategory"),
          // @ts-expect-error incompatible param.
          rules: { required: t("validation:requiredCategoryTitle"), validate: validateUniqueParentCategory(t) },
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
          // @ts-expect-error incompatible param.
          rules: { validate: validateUniqueCategoryType(t) },
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
          valueType: "authors-additions",
          name: "candidatesIds",
          placeholder: t("selectAuthorsToAdd"),
        },
        {
          valueType: "authors-removals",
          name: "authorsIds",
          placeholder: t("selectAuthorsToRemove"),
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
      operationType: "site-moderator-addition",
      fields: [
        {
          valueType: "moderators-additions",
          name: "candidatesIds",
          placeholder: t("selectModeratorsToAdd"),
        },
      ],
    },
    {
      operationType: "site-moderator-removal",
      fields: [
        {
          valueType: "moderators-removals",
          name: "moderatorsIds",
          placeholder: t("selectModeratorsToRemove"),
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
          // @ts-expect-error incompatible param.
          rules: { validate: validateUniqueSiteNickname(t) },
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
