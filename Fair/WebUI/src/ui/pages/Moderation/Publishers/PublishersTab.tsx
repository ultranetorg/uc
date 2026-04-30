import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { useGetSitePublishers } from "entities"
import { Table, TableEmptyState } from "ui/components"

import { getPublishersTabItemRenderer } from "./publishersTabItemRenderer"

export const PublishersTab = () => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation("publishersPage")
  const voterId = getOperationVoterId("site-authors-removal")

  const { data: publishers } = useGetSitePublishers(siteId)

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:author"), type: "author", className: "w-[45%]" },
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

  const itemRenderer = useMemo(() => getPublishersTabItemRenderer(t, siteId!), [siteId, t])

  const items = useMemo(
    () =>
      publishers?.map(x => ({
        id: x.author.id,
        ...x,
      })),
    [publishers],
  )

  return (
    <Table
      columns={columns}
      items={items}
      itemRenderer={itemRenderer}
      tableBodyClassName="text-2sm leading-5"
      emptyState={<TableEmptyState message={t("noPublishers")} />}
    />
  )
}
