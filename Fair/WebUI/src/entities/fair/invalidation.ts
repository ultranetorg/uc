import { useCallback } from "react"
import { QueryKey, useQueryClient } from "@tanstack/react-query"

import { OperationType } from "types"

import { categoriesKeys } from "./categories"
import { moderationKeys } from "./moderation"
import { publicationsKeys } from "./publications"
import { reviewKeys } from "./publicationReviews"
import { storesKeys } from "./stores"
import { usersKeys } from "./users"

export type InvalidationContext = {
  storeId: string
  publicationId?: string
  userName?: string
  proposalId?: string
}

type InvalidationRule = (ctx: InvalidationContext) => QueryKey[]

// Любое модераторское или паблишерское изменение инвалидирует только moderationKeys.root.
const operationRules: Record<OperationType, InvalidationRule> = {
  "category-avatar-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree
  "category-creation": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-deletion": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-movement": ({ storeId }) => [categoriesKeys.all(storeId)],
  "category-type-change": ({ storeId }) => [categoriesKeys.all(storeId)], // TODO: update all except tree

  "publication-creation": ({ storeId }) => [storesKeys.detail(storeId), moderationKeys.root(storeId)],
  "publication-deletion": ({ storeId }) => [
    storesKeys.detail(storeId),
    moderationKeys.root(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
  ],
  "publication-publish": ({ storeId }) => [
    storesKeys.detail(storeId),
    moderationKeys.root(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
  ],
  "publication-unpublish": ({ storeId }) => [
    storesKeys.detail(storeId),
    moderationKeys.root(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
  ],
  "publication-updation": ({ storeId, publicationId }) => [
    storesKeys.detail(storeId),
    moderationKeys.root(storeId),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
    publicationsKeys.detail(publicationId!),
  ],

  "review-creation": ({ storeId, publicationId }) => [
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.detail(publicationId!),
    reviewKeys.all(publicationId!),
    moderationKeys.root(storeId),
  ],
  "review-edit": ({ storeId, publicationId }) => [
    publicationsKeys.detail(publicationId!),
    reviewKeys.all(publicationId!),
    moderationKeys.root(storeId),
  ],
  // The publication id may be unknown (e.g. voting from the proposal page), then everything related to
  // publications is refreshed: the reviews lists and the rating shown in the publication details.
  "review-deletion": ({ storeId, publicationId }) => [
    publicationId ? reviewKeys.all(publicationId) : reviewKeys.root(),
    publicationId ? publicationsKeys.detail(publicationId) : publicationsKeys.root(),
    publicationsKeys.categoriesPublications(storeId),
    publicationsKeys.categoriesPublicationsAll(),
    moderationKeys.root(storeId),
  ],

  "store-authors-removal": ({ storeId }) => [storesKeys.publishers(storeId), moderationKeys.root(storeId)],
  "store-moderator-addition": ({ storeId }) => [storesKeys.moderators(storeId), moderationKeys.root(storeId)],
  "store-moderator-removal": ({ storeId }) => [storesKeys.moderators(storeId), moderationKeys.root(storeId)],

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
  "user-unregistration": ({ storeId }) => [storesKeys.users(storeId), moderationKeys.root(storeId)],
}

// Смена политики голосования (policy survey) для любой операции.
const policyChangeRule: InvalidationRule = ({ storeId }) => [storesKeys.policies(storeId), moderationKeys.root(storeId)]

// Любое изменение предложения (создание, голосование, комментарий). Создание предложения с голосованием не выполняет
// операцию, поэтому правило операции не вызывается, а списки должны обновиться.
const proposalRule: InvalidationRule = ({ storeId }) => [moderationKeys.root(storeId)]

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

  const invalidateProposal = useCallback(
    (ctx: InvalidationContext) => invalidateKeys(proposalRule(ctx)),
    [invalidateKeys],
  )

  return { invalidateOperation, invalidatePolicyChange, invalidateProposal }
}
