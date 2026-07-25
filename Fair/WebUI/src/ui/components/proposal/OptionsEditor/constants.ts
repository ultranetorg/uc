import { TFunction } from "i18next"

import { CreateProposalData, ProductType, ReviewStatus } from "types"

import { EditorOperationFields } from "./types"
import {
  validateUniqueCategoryTitle,
  validateUniqueCategoryType,
  validateUniqueFileId,
  validateUniqueParentCategory,
  validateUniqueStoreName,
} from "./validations"

const atLeastOneStoreInfoField = (_: unknown, formValues: CreateProposalData) =>
  (formValues.options || []).every(
    opt =>
      ((opt.storeTitle ?? "") as string).trim().length > 0 ||
      ((opt.slogan ?? "") as string).trim().length > 0 ||
      ((opt.description ?? "") as string).trim().length > 0,
  )

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
          placeholder: t("placeholders:selectImage"),
          // @ts-expect-error incompatible param.
          rules: { required: t("validation:requiredFile"), validate: validateUniqueFileId(t) },
        },
      ],
    },
    {
      operationType: "category-creation",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:parentCategory"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      parameterRules: { required: false },
      parameterHasRoot: true,
      fields: [
        {
          valueType: "string",
          name: "categoryTitle",
          placeholder: t("placeholders:enterCategoryTitle"),
          rules: {
            required: t("validation:requiredCategoryTitle"),
            maxLength: { value: 64, message: t("validation:maxLength", { count: 64 }) },
            validate: validateUniqueCategoryTitle(t),
          },
        },
      ],
    },
    {
      operationType: "category-movement",
      parameterValueType: "category",
      parameterName: "categoryId",
      parameterLabel: t("common:category"),
      parameterPlaceholder: t("placeholders:selectCategory"),
      parameterRules: { validate: validateUniqueParentCategory(t) },
      fields: [
        {
          valueType: "category-root",
          name: "parentCategoryId",
          placeholder: t("placeholders:selectParentCategory"),
          rules: { required: false, validate: validateUniqueParentCategory(t) },
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

    // Store
    {
      operationType: "store-authors-removal",
      fields: [
        {
          valueType: "authors-removals",
          name: "authors",
        },
      ],
    },
    {
      operationType: "store-avatar-change",
      fields: [
        {
          valueType: "file",
          name: "fileId",
          // @ts-expect-error incompatible param.
          rules: { required: t("validation:requiredFile"), validate: validateUniqueFileId(t) },
        },
      ],
    },
    {
      operationType: "store-moderator-addition",
      fields: [
        {
          valueType: "moderators-additions",
          name: "moderators",
          placeholder: t("selectModeratorsToAdd"),
        },
      ],
    },
    {
      operationType: "store-moderator-removal",
      fields: [
        {
          valueType: "moderators-removals",
          name: "moderators",
          placeholder: t("selectModeratorsToRemove"),
        },
      ],
    },
    {
      operationType: "store-name-change",
      fields: [
        {
          valueType: "string",
          name: "name",
          placeholder: t("placeholders:enterName"),
          rules: {
            required: t("validation:required"),
            minLength: { value: 5, message: t("validation:minLength", { count: 5 }) },
            maxLength: { value: 32, message: t("validation:maxLength", { count: 32 }) },
            pattern: { value: /^[a-z0-9_]+$/, message: t("validation:onlyLowercaseLatinNumbersAndUnderscores") },
            validate: validateUniqueStoreName(t),
          },
        },
      ],
    },
    {
      operationType: "store-info-updation",
      fields: [
        {
          valueType: "string",
          name: "storeTitle",
          placeholder: t("placeholders:enterTitle"),
          rules: { required: false, validate: { atLeastOneField: atLeastOneStoreInfoField } },
        },
        {
          valueType: "string",
          name: "slogan",
          placeholder: t("placeholders:enterSlogan"),
          rules: { required: false, validate: { atLeastOneField: atLeastOneStoreInfoField } },
        },
        {
          valueType: "string-multiline",
          name: "description",
          placeholder: t("placeholders:enterDescription"),
          rules: { required: false, validate: { atLeastOneField: atLeastOneStoreInfoField } },
        },
      ],
    },

    // User
    {
      operationType: "user-unregistration",
      parameterValueType: "user",
      parameterName: "userId",
      parameterLabel: t("common:user"),
    },
  ] as const
