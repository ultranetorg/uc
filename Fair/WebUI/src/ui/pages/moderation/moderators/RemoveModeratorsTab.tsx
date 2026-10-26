import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"
import { TFunction } from "i18next"

import { useStoreRolesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetModeratorSurveys, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { StoreModeratorRemoval, Survey, SurveyVoting } from "types"
import { ButtonPrimary, Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderUser } from "ui/renderers2"
import { isPublisherVotedInSurvey, parseInteger, showToast } from "utils"

const getColumns = (t: TFunction, canVote: boolean) => [
  {
    accessor: "account",
    label: t("common:moderators"),
    type: "account",
    className: "first-letter:uppercase w-1/5",
  },
  {
    accessor: "totalVotes",
    label: t("totalVotes"),
    className: "w-1/5 first-letter:uppercase text-right",
  },
  {
    accessor: "votesRequiredToWin",
    label: t("votesRequired"),
    className: "w-1/5 first-letter:uppercase text-right",
  },
  ...(canVote
    ? [
        {
          accessor: "actions",
          label: t("common:actions"),
          type: "actions",
          className: "w-1/5 first-letter:uppercase text-center",
        },
      ]
    : []),
]

const getItemRenderer =
  (t: TFunction, onVote: (surveyId: string) => void, votingSurveyId?: string, publisherId?: string) =>
  (item: TableItem, column: TableColumn) => {
    const survey = item as unknown as Survey

    switch (column.type) {
      case "account": {
        const option = survey.options[0].operation as unknown as StoreModeratorRemoval
        return renderUser(option.moderator.id, option.moderator.name)
      }

      case "actions":
        return (
          <div className="flex justify-center">
            <ButtonPrimary
              className="h-9 w-20 capitalize"
              label={t("common:remove")}
              loading={votingSurveyId === survey.id}
              disabled={!publisherId || votingSurveyId === survey.id || isPublisherVotedInSurvey(survey, publisherId!)}
              onClick={e => {
                onVote(survey.id)
                e.stopPropagation()
                e.preventDefault()
              }}
            />
          </div>
        )
    }
  }

export const RemoveModeratorsTab = () => {
  const { mutate } = useTransactMutationWithStatus()
  const { t } = useTranslation("moderatorsPage")

  const { invalidateOperation } = useInvalidation()
  const storeId = useResolveStoreId()
  const { publisherIds } = useStoreRolesContext()
  const publisherId = publisherIds && publisherIds[0]

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)
  const [votingSurveyId, setVotingSurveyId] = useState<string>()

  const { data: surveys, refetch } = useGetModeratorSurveys(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    surveys?.totalItems && surveys.totalItems > 0 ? Math.ceil(surveys.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handleVote = useCallback(
    (surveyId: string) => {
      setVotingSurveyId(surveyId)
      const operation = new SurveyVoting(storeId!, surveyId!, publisherId!, 0)
      mutate(operation, {
        onSuccess: () => {
          invalidateOperation("store-moderator-removal", { storeId: storeId! })
          showToast(t("toast:surveyVoted", { publisher: publisherId }))
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: async () => {
          await refetch()
          setVotingSurveyId(undefined)
        },
      })
    },
    [storeId, publisherId, mutate, invalidateOperation, t, refetch],
  )

  const columns = useMemo(() => getColumns(t, publisherIds !== undefined && publisherIds.length > 0), [publisherIds, t])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  const itemRenderer = useMemo(
    () => getItemRenderer(t, handleVote, votingSurveyId, publisherId!),
    [handleVote, publisherId, t, votingSurveyId],
  )

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={surveys?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noModeratorSurveys")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
