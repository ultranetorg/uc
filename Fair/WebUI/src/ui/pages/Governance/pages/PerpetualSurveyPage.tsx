import { useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useUserContext } from "app"
import { useGetPerpetualSurveyDetails } from "entities/PerpetualSurveys"
import { useTransactMutationWithStatus } from "entities/node"
import { PerpetualVoting, SiteApprovalPolicyChange } from "types"
import { Breadcrumbs } from "ui/components"
import { OptionsCollapsesList, OptionsCollapsesListItem } from "ui/components/proposal"
import { showToast } from "utils"

export type PageState = "voting" | "results"

export const PerpetualSurveyPage = () => {
  const { t } = useTranslation("perpetualSurvey")
  const { siteId, perpetualSurveyId } = useParams()

  const { isPublisher, publishersIds } = useUserContext()
  const { mutate, isPending } = useTransactMutationWithStatus()

  const [items, setItems] = useState<OptionsCollapsesListItem[] | undefined>()

  const { data: survey, isFetching, refetch } = useGetPerpetualSurveyDetails(siteId, perpetualSurveyId)

  const operation = (survey?.options[0].operation as SiteApprovalPolicyChange)?.operation
  const title = operation !== undefined ? t(`operations:${operation}`) : undefined

  const handleExpand = useCallback(
    (value: string | number, expanded: boolean) =>
      setItems(p => p?.map(x => (x.value !== value ? x : { ...x, expanded }))),
    [],
  )

  const handleVote = useCallback(
    (choiceId: string | number) => {
      const publisherId = publishersIds![0]
      const operation = new PerpetualVoting(siteId!, Number(perpetualSurveyId), publisherId, Number(choiceId))
      mutate(operation, {
        onSuccess: () => showToast(t("toast:perpetualVoted", { publisher: publisherId })),
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => refetch(),
      })
    },
    [mutate, perpetualSurveyId, publishersIds, refetch, siteId, t],
  )
  useEffect(() => {
    if (!survey) return

    setItems(prevItems => {
      return survey.options.map((x, i) => {
        const operation = x.operation as SiteApprovalPolicyChange

        const prevItem = prevItems?.find(item => item.value === i)

        return {
          ...prevItem,
          title: t(`approvalRequirement:${operation.approval}`),
          description: t(operation.approval),
          value: i,
          votePercents: x.votePercents,
          votesCount: x.yesVotes.length,
          voted: publishersIds && publishersIds.length > 0 ? x.yesVotes.includes(publishersIds[0]) : prevItem?.voted,
        }
      })
    })
  }, [publishersIds, survey, t])

  if (!survey || isFetching) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: t("home") },
            { title: t("common:governance"), path: `/${siteId}/g` },
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
            disabled={isPending}
            items={items!}
            showResults={true}
            showVoteButton={isPublisher}
            votesText={t("common:votes")}
            onExpand={handleExpand}
            onVoteClick={handleVote}
          />
        </div>
      </div>
    </div>
  )
}
