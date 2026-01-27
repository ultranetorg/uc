import { useCallback, useMemo } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetPerpetualSurveys } from "entities/perpetualSurveys"
import { Table, TableEmptyState } from "ui/components"
import { perpetualSurveysItemRenderer } from "ui/renderers"

export const PerpetualSurveysTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("governance")
  const navigate = useNavigate()

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
        label: t("votedApproval"),
        type: "voted-approval",
        className: "w-1/5 first-letter:uppercase",
      },
      {
        accessor: "totalVotes",
        label: t("totalVotes"),
        className: "w-1/5 first-letter:uppercase",
      },
      {
        accessor: "votesRequiredToWin",
        label: t("votesRequired"),
        className: "w-1/5 first-letter:uppercase",
      },
      {
        accessor: "winPercentage",
        label: t("winPercentage"),
        type: "win-percentage",
        className: "w-1/5 first-letter:uppercase",
      },
    ],
    [t],
  )

  const { data: surveys } = useGetPerpetualSurveys(siteId)

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/g/p/${id}`), [navigate, siteId])

  const itemRenderer = useMemo(() => perpetualSurveysItemRenderer(t), [t])

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={surveys}
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noPerpetualSurveys")} />}
        onRowClick={handleTableRowClick}
      />
    </div>
  )
}
