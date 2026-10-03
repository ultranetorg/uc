import { useCallback } from "react"
import { QueryKey, useQueryClient } from "@tanstack/react-query"

import { OperationType } from "types"

import { categoriesKeys } from "./categories"
import { perpetualSurveysKeys } from "./perpetualSurveys"
import { proposalsKeys } from "./proposals"
import { publicationsKeys } from "./publications"
import { storesKeys } from "./stores"
import { unpublishedPublicationsKeys } from "./unpublishedPublications"
import { usersKeys } from "./users"

export type InvalidationContext = {
  storeId: string
  publicationId?: string
  userName?: string
}

type InvalidationRule = (ctx: InvalidationContext) => QueryKey[]

const operationRules: Record<OperationType, InvalidationRule> = {
  "category-avatar-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree
  "category-creation": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-deletion": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-movement": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-type-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree

  "publication-creation": ({ storeId }) => [
    storesKeys.detail(storeId),
    unpublishedPublicationsKeys.all(storeId),
    proposalsKeys.publications(storeId),
  ],
  "publication-deletion": ({ storeId }) => [
    storesKeys.detail(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
  ],
  "publication-publish": ({ storeId }) => [
    storesKeys.detail(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
    unpublishedPublicationsKeys.all(storeId),
  ],
  "publication-unpublish": ({ storeId }) => [
    storesKeys.detail(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
    unpublishedPublicationsKeys.all(storeId),
  ],
  "publication-updation": ({ storeId, publicationId }) => [
    storesKeys.detail(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
    publicationsKeys.detail(publicationId!),
    publicationsKeys.changedPublications(storeId),
  ],

  "review-creation": ({ storeId, publicationId }) => [
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.detail(publicationId!),
  ],
  "review-edit": ({ publicationId }) => [publicationsKeys.detail(publicationId!)],
  "review-status-change": () => [],

  "store-authors-removal": ({ storeId }) => [storesKeys.publishers(storeId), proposalsKeys.publishers(storeId)],
  "store-moderator-addition": ({ storeId }) => [storesKeys.moderators(storeId), proposalsKeys.moderators(storeId!)],
  "store-moderator-removal": ({ storeId }) => [storesKeys.moderators(storeId), proposalsKeys.moderators(storeId)],

  "store-avatar-change": ({ storeId, userName }) => [
    storesKeys.detail(storeId),
    ...(userName ? [usersKeys.detail(userName)] : []),
  ],
  "store-renaming": ({ storeId }) => [storesKeys.detail(storeId)],
  "store-info-updation": ({ storeId, userName }) => [
    storesKeys.detail(storeId),
    ...(userName ? [usersKeys.detail(userName)] : []),
  ],

  "user-registration": ({ storeId }) => [storesKeys.users(storeId)],
  "user-unregistration": ({ storeId }) => [storesKeys.users(storeId), proposalsKeys.userUnregistrations(storeId)],
}

// Смена политики голосования (perpetual survey) для любой операции.
const policyChangeRule: InvalidationRule = ({ storeId }) => [
  storesKeys.policies(storeId),
  perpetualSurveysKeys.all(storeId),
]

export const useInvalidation = () => {
  const queryClient = useQueryClient()

  const invalidateKeys = useCallback(
    (keys: QueryKey[]) => keys.forEach(queryKey => queryClient.invalidateQueries({ queryKey, refetchType: "all" })),
    [queryClient],
  )

  const invalidateOperation = useCallback(
    (type: OperationType | undefined, ctx: InvalidationContext) => {
      if (type) invalidateKeys(operationRules[type](ctx))
    },
    [invalidateKeys],
  )

  const invalidatePolicyChange = useCallback(
    (ctx: InvalidationContext) => invalidateKeys(policyChangeRule(ctx)),
    [invalidateKeys],
  )

  return { invalidateOperation, invalidatePolicyChange }
}
