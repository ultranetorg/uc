import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetSitePublishers } from "entities"
import { Table, TableEmptyState } from "ui/components"

export const PublishersTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("publishersTab")

  const { data: publishers } = useGetSitePublishers(siteId)

  const columns = useMemo(
    () => [
      { accessor: "account", label: t("common:user"), className: "w-[60%]" },
      { accessor: "bannedTill", label: t("bannedTill"), className: "w-[40%]" },
    ],
    [t],
  )

  const items = useMemo(
    () =>
      publishers?.map(m => ({
        id: m.author.id,
        account: m.author.id,
        bannedTill: m.bannedTill === 0 ? "-" : new Date(m.bannedTill * 1000).toLocaleDateString(),
      })),
    [publishers],
  )

  return (
    <Table
      columns={columns}
      items={items}
      tableBodyClassName="text-2sm leading-5"
      emptyState={<TableEmptyState message={t("noModerators")} />}
    />
  )
}
