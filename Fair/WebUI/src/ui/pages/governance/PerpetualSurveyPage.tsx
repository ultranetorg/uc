import { useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { startCase } from "lodash"

import { useSignInContext, useStoreContext, useStoreRolesContext, useUserContext } from "app"
import { useGetPerpetualSurveyDetails, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { PerpetualVoting, StoreApprovalPolicyChange } from "types"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"
import { routes, showToast } from "utils"

export type PageState = "voting" | "results"

export const PerpetualSurveyPage = () => {
  const { t } = useTranslation("perpetualSurveyPage")
  const { perpetualSurveyId } = useParams()
  const { invalidatePolicyChange } = useInvalidation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { user } = useUserContext()

  const { startSignIn, openAuthorRoleRequiredModal } = useSignInContext()
  const { publisherIds } = useStoreRolesContext()
  const { mutate } = useTransactMutationWithStatus()

  const [items, setItems] = useState<OptionsCollapsesListItem[] | undefined>()

  const { data: survey, isFetching, refetch } = useGetPerpetualSurveyDetails(storeId, perpetualSurveyId)

  const operation = (survey?.options[0].operation as StoreApprovalPolicyChange)?.operation
  const title = operation !== undefined ? t(`operations:${operation}`) : undefined

  useStoreTitle(store?.title, `Perpetual Survey - ${startCase(title)}`)

  const handleExpand = useCallback(
    (value: string | number, expanded: boolean) =>
      setItems(p => p?.map(x => (x.value !== value ? x : { ...x, expanded }))),
    [],
  )

  const handleSignInOrVote = useCallback(
    (choiceId: string | number) => {
      if (!publisherIds || !publisherIds.length) {
        if (!user) {
          startSignIn("author")
        } else {
          openAuthorRoleRequiredModal()
        }

        return
      }

      const publisherId = publisherIds![0]
      const operation = new PerpetualVoting(storeId!, Number(perpetualSurveyId), publisherId, Number(choiceId))
      mutate(operation, {
        onSuccess: () => {
          invalidatePolicyChange({ storeId: storeId! })

          showToast(t("toast:perpetualVoted", { publisher: publisherId }))
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => refetch(),
      })
    },
    [
      publisherIds,
      storeId,
      perpetualSurveyId,
      mutate,
      user,
      startSignIn,
      openAuthorRoleRequiredModal,
      invalidatePolicyChange,
      t,
      refetch,
    ],
  )

  useEffect(() => {
    if (!survey) return

    setItems(prevItems => {
      return survey.options.map((x, i) => {
        const operation = x.operation as StoreApprovalPolicyChange

        const prevItem = prevItems?.find(item => item.value === i)

        return {
          ...prevItem,
          title: t(`approvalRequirement:${operation.approval}`),
          description: t(operation.approval),
          value: i,
          votePercents: x.votePercents,
          votesCount: x.yesVotes.length,
          voted: publisherIds && publisherIds.length > 0 ? x.yesVotes.includes(publisherIds[0]) : prevItem?.voted,
        }
      })
    })
  }, [publisherIds, survey, t])

  if (!survey || isFetching) {
    return <div>Loading{import.meta.env.DEV ? " (PerpetualSurveyPage)" : ""}</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.store(storeId!), title: t("home") },
            { title: t("common:perpetualSurveys"), path: routes.governance.surveys(storeId!) },
            { title: title! },
          ]}
        />
        <div className="flex flex-col gap-4">
          <span className="text-3.5xl font-semibold leading-10">{t("title", { operation: title })}</span>
        </div>
      </div>
      <div className="flex gap-8">
        <div className="flex w-full max-w-187.5 flex-col gap-8">
          <OptionsCollapsesList
            items={items!}
            showResults={true}
            showVoteButton={true}
            votesText={t("common:votes")}
            onExpand={handleExpand}
            onVoteClick={handleSignInOrVote}
            expandAll={true}
          />
        </div>
      </div>
    </div>
  )
}
