import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetSiteModerators } from "entities"
import { Table, TableEmptyState } from "ui/components"
import { useModerationContext } from "app"
import { getItemRenderer } from "./renderer"

export const ModeratorsTab = () => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation("moderatorsPage")
  const voterId = getOperationVoterId("site-moderator-removal")

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
              className: "w-[15%] first-letter:uppercase",
            },
          ]
        : []),
    ],
    [t, voterId],
  )

  const itemRenderer = useMemo(() => getItemRenderer(t, siteId!), [siteId, t])

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
