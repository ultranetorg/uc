import { useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useGetSiteModerators } from "entities"
import { useResolveSiteId } from "hooks"
import { Table, TableEmptyState } from "ui/components"
import { useOperationPolicy } from "app"

import { moderatorsTabItemRenderer } from "./moderatorsTabItemRenderer"

export const ModeratorsTab = () => {
  const siteId = useResolveSiteId()
  const { voterId } = useOperationPolicy("site-moderator-removal")
  const { t } = useTranslation("moderatorsPage")

  const { data: moderators } = useGetSiteModerators(siteId)

  const columns = useMemo(
    () => [
      {
        accessor: "account",
        label: t("common:moderators"),
        type: "account",
        className: "first-letter:uppercase w-[45%]",
      },
      { accessor: "bannedTill", label: t("bannedTill"), type: "banned", className: "w-[40%]" },
      ...(voterId
        ? [
            {
              accessor: "actions",
              label: t("common:actions"),
              type: "actions",
              className: "w-[10%] text-center first-letter:uppercase",
            },
          ]
        : []),
    ],
    [t, voterId],
  )

  const itemRenderer = useMemo(() => moderatorsTabItemRenderer(t, siteId!), [siteId, t])

  const items = useMemo(
    () =>
      moderators?.map(x => ({
        id: x.user.id,
        ...x,
      })),
    [moderators],
  )

  return (
    <Table
      columns={columns}
      items={items}
      itemRenderer={itemRenderer}
      tableBodyClassName="text-2sm leading-5"
      emptyState={<TableEmptyState message={t("noModerators")} />}
    />
  )
}
