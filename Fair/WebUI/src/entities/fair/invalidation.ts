import { useCallback } from "react"
import { QueryKey, useQueryClient } from "@tanstack/react-query"

import { OperationType } from "types"

import { categoriesKeys } from "./categories"
import { proposalsKeys } from "./proposals"
import { publicationsKeys } from "./publications"
import { storesKeys } from "./stores"
import { unpublishedPublicationsKeys } from "./UnpublishedPublications"
import { usersKeys } from "./users"

export type InvalidationContext = {
  storeId: string
  userName?: string
}

type InvalidationRule = (ctx: InvalidationContext) => QueryKey[]

const operationRules: Record<OperationType, InvalidationRule> = {
  "category-avatar-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree
  "category-creation": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-deletion": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-movement": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-type-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree

  "publication-creation": () => [],
  "publication-deletion": ({ storeId }) => [publicationsKeys.categoriesPublications(storeId)],
  "publication-publish": ({ storeId }) => [unpublishedPublicationsKeys.all(storeId)],
  "publication-unpublish": ({ storeId }) => [
    unpublishedPublicationsKeys.all(storeId),
    publicationsKeys.categoriesPublications(storeId),
  ],
  "publication-updation": () => [],

  "review-creation": () => [],
  "review-edit": () => [],
  "review-status-change": () => [],

  "store-authors-removal": ({ storeId }) => [storesKeys.publishers(storeId), proposalsKeys.publishers(storeId)],
  "store-moderator-addition": ({ storeId }) => [storesKeys.moderators(storeId)],
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
const policyChangeRule: InvalidationRule = ({ storeId }) => [storesKeys.policies(storeId)]

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
