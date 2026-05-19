import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { useGetSitePolicies, useGetPerpetualSurveys } from "entities"
import { Table, TableEmptyState } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { perpetualSurveysItemRenderer } from "ui/renderers"

export const PerpetualSurveysPage = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()
  const { t } = useTranslation("perpetualSurveysPage")

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
  const { data: policies } = useGetSitePolicies(true, siteId)

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/g/p/${id}`), [navigate, siteId])

  const itemRenderer = useMemo(() => perpetualSurveysItemRenderer(t, policies), [policies, t])

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={t("title")} breadcrumbTitle={t("common:governance")} />
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
