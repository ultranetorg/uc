import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetSitePublishers } from "entities"
import { Table, TableEmptyState } from "ui/components"

import { getItemRenderer } from "./renderer"

export const PublishersTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("publishersPage")

  const { data: publishers } = useGetSitePublishers(siteId)

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:author"), type: "author", className: "w-[60%]" },
      { accessor: "bannedTill", label: t("bannedTill"), className: "w-[40%]" },
    ],
    [t],
  )
  const itemRenderer = getItemRenderer(t)

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
      emptyState={<TableEmptyState message={t("noModerators")} />}
    />
  )
}
