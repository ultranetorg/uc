import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useSiteContext } from "app"
import { useGetSitePolicies, useGetPerpetualSurveys } from "entities"
import { useResolveSiteId, useSiteTitle } from "hooks"
import { Table, TableEmptyState } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { perpetualSurveysItemRenderer } from "ui/renderers"
import { routes } from "utils"

export const PerpetualSurveysPage = () => {
  const siteId = useResolveSiteId()
  const navigate = useNavigate()
  const { site } = useSiteContext()
  const { t } = useTranslation("perpetualSurveysPage")

  useSiteTitle(site?.title, "Perpetual Surveys")

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

  const { data: surveys } = useGetPerpetualSurveys(siteId)
  const { data: policies } = useGetSitePolicies(true, siteId)

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.governance.survey(siteId!, id)),
    [navigate, siteId],
  )

  const itemRenderer = useMemo(() => perpetualSurveysItemRenderer(t, policies), [policies, t])

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={t("title")} />
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
