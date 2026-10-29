import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useStoreContext, useStoreRolesContext } from "app"
import { useGetStorePolicies, useGetPolicySurveys as useGetPolicySurveys } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { StoreApprovalPolicyChange, Survey } from "types"
import { Table, TableEmptyState, TableItem } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { routes } from "utils"

import { surveysItemRenderer } from "./surveysItemRenderer"

const CATEGORIES_ORDER = ["store", "category", "publication", "review", "user"]

export const PolicySurveysPage = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const { store } = useStoreContext()
  const { publisherIds } = useStoreRolesContext()
  const { t } = useTranslation("policySurveysPage")

  useStoreTitle(store?.title, "Policy Surveys")

  const columns = useMemo(
    () => [
      {
        accessor: "operation",
        label: t("columns:operation"),
        type: "operation",
        className: "w-1/4",
      },
      {
        accessor: "votedApproval",
        label: t("columns:currentPolicy"),
        type: "voted-approval",
        className: "w-1/5",
      },
      {
        accessor: "leadingOption",
        label: t("columns:leadingOption"),
        type: "leading-option",
        className: "w-1/5",
      },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.title"),
        type: "progress",
        className: "w-1/6",
      },
      {
        accessor: "yourVote",
        label: t("columns:yourVote"),
        type: "your-vote",
        className: "w-1/5",
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

  const itemRenderer = useMemo(() => surveysItemRenderer(t, policies, publisherIds), [policies, publisherIds, t])

  const groups = useMemo(() => {
    const map = new Map<string, Survey[]>()
    for (const survey of surveys ?? []) {
      const operation = (survey.options?.[0]?.operation as StoreApprovalPolicyChange | undefined)?.operation
      const category = operation?.split("-")[0] ?? "other"
      map.set(category, [...(map.get(category) ?? []), survey])
    }
    return [...map.entries()].sort(
      ([a], [b]) => (CATEGORIES_ORDER.indexOf(a) + 1 || 99) - (CATEGORIES_ORDER.indexOf(b) + 1 || 99),
    )
  }, [surveys])

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={t("title")} />
      {groups.length === 0 && (
        <Table
          columns={columns}
          items={[]}
          itemRenderer={itemRenderer}
          emptyState={<TableEmptyState message={t("noSurveys")} />}
        />
      )}
      {groups.map(([category, items]) => (
        <div key={category} className="flex flex-col gap-3">
          <span className="text-xl font-semibold">{t(`categories.${category}`, { defaultValue: category })}</span>
          <Table
            columns={columns}
            items={items}
            itemRenderer={itemRenderer}
            emptyState={<TableEmptyState message={t("noSurveys")} />}
            onRowClick={handleTableRowClick}
            isRowClickable={isRowClickable}
          />
        </div>
      ))}
    </div>
  )
}
