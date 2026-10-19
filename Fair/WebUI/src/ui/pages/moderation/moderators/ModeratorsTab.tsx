import { ReactNode, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useGetStoreModerators } from "entities"
import { useResolveStoreId } from "hooks"
import { Moderator } from "types"
import { Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderUser } from "ui/renderers2"
import { formatDate } from "utils"

const moderatorsTabItemRenderer = (item: TableItem, column: TableColumn): ReactNode => {
  const moderator = item as unknown as Moderator

  switch (column.type) {
    case "account":
      return renderUser(moderator.user.id, moderator.user.nickname)

    case "banned":
      return moderator.bannedTill !== 0 ? formatDate(moderator.bannedTill) : ""
  }
}

export const ModeratorsTab = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("moderatorsPage")

  const { data: moderators } = useGetStoreModerators(storeId)

  const columns = useMemo(
    () => [
      {
        accessor: "account",
        label: t("common:moderators"),
        type: "account",
        className: "first-letter:uppercase w-[45%]",
      },
      { accessor: "bannedTill", label: t("bannedTill"), type: "banned", className: "w-[40%]" },
    ],
    [t],
  )

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
      itemRenderer={moderatorsTabItemRenderer}
      tableBodyClassName="text-2sm leading-5"
      emptyState={<TableEmptyState message={t("noModerators")} />}
    />
  )
}
