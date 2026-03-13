import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetSiteModerators } from "entities"
import { Table, TableEmptyState } from "ui/components"

export const ModeratorsTab = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("moderatorsPage")

  const { data: moderators } = useGetSiteModerators(siteId)

  const columns = useMemo(
    () => [
      { accessor: "account", label: t("common:user"), className: "w-[60%]" },
      { accessor: "bannedTill", label: t("bannedTill"), className: "w-[40%]" },
    ],
    [t],
  )

  const items = useMemo(
    () =>
      moderators?.map(m => ({
        id: m.user.id,
        account: m.user.address,
        bannedTill: m.bannedTill === 0 ? "-" : new Date(m.bannedTill * 1000).toLocaleDateString(),
      })),
    [moderators],
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
