import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useGetStorePolicies, useGetPolicySurveys } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { Table, TableEmptyState } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { surveysItemRenderer } from "ui/renderers"
import { routes } from "utils"

export const ModeratorSurveysPage = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const { store } = useStoreContext()
  const { t } = useTranslation("surveysPage")

  useStoreTitle(store?.title, "Policy Surveys")

  const columns = useMemo(
    () => [
      {
        accessor: "operation",
        label: t("common:operation"),
        type: "operation",
        className: "w-2/5 first-letter:uppercase",
      },
      {
        accessor: "votedApproval",
        label: t("currentPolicy"),
        type: "voted-approval",
        className: "w-1/5 first-letter:uppercase",
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
      {
        accessor: "winPercentage",
        label: t("winPercentage"),
        type: "win-percentage",
        className: "w-1/5 first-letter:uppercase text-right",
      },
    ],
    [t],
  )

  const { data: surveys } = useGetPolicySurveys(storeId)
  const { data: policies } = useGetStorePolicies(true, storeId)

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.governance.policySurveyDetails(storeId!, id)),
    [navigate, storeId],
  )

  const itemRenderer = useMemo(() => surveysItemRenderer(t, policies), [policies, t])

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={t("title")} />
      <Table
        columns={columns}
        items={surveys}
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noSurveys")} />}
        onRowClick={handleTableRowClick}
      />
    </div>
  )
}
