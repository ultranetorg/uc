import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext } from "app"
import { useGetStorePolicies, useGetPolicySurveys as useGetPolicySurveys } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { Survey } from "types"
import { Table, TableEmptyState, TableItem } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { routes } from "utils"

import { surveysItemRenderer } from "./surveysItemRenderer"

export const PolicySurveysPage = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const { store } = useStoreContext()
  const { t } = useTranslation("policySurveysPage")

  useStoreTitle(store?.title, "Policy Surveys")

  const columns = useMemo(
    () => [
      {
        accessor: "operation",
        label: t("columns:operation"),
        type: "operation",
        className: "w-2/5",
      },
      {
        accessor: "votedApproval",
        label: t("columns:currentPolicy"),
        type: "voted-approval",
        className: "w-1/5",
      },
      {
        accessor: "totalVotes",
        label: t("columns:totalVotes"),
        className: "w-1/5 text-right",
      },
      {
        accessor: "votesRequiredToWin",
        label: t("columns:votesRequired"),
        className: "w-1/5 text-right",
      },
      {
        accessor: "winPercentage",
        label: t("columns:winPercentage"),
        type: "win-percentage",
        className: "w-1/5 text-right",
      },
    ],
    [t],
  )

  const { data: surveys } = useGetPolicySurveys(storeId)
  const { data: policies } = useGetStorePolicies(true, storeId)

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.policySurveyDetails(storeId!, id)),
    [navigate, storeId],
  )

  const isRowClickable = useCallback((item: TableItem) => (item as Survey).isVariableApproval === true, [])

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
        isRowClickable={isRowClickable}
      />
    </div>
  )
}
