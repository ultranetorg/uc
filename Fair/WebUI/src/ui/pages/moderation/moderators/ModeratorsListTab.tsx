import { ReactNode, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"

import { useUserContext } from "app"
import { useGetStoreModerators } from "entities"
import { useResolveStoreId } from "hooks"
import { Moderator } from "types"
import { Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderBannedStatus } from "ui/renderers"

import { renderModerator } from "./utils"

const getItemRenderer =
  (t: TFunction, currentUserId?: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const moderator = item as unknown as Moderator

    switch (column.type) {
      case "account":
        return renderModerator(t, moderator.user.id, moderator.user.nickname, currentUserId)

      case "status":
        return renderBannedStatus(t, moderator.bannedTill)
    }
  }

export const ModeratorsListTab = () => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation("moderatorsPage")
  const { user } = useUserContext()

  const { data: moderators } = useGetStoreModerators(storeId)

  const columns = useMemo(
    () => [
      {
        accessor: "account",
        label: t("columns:moderators"),
        type: "account",
        className: "w-[85%]",
      },
      { accessor: "bannedTill", label: t("columns:status"), type: "status", className: "w-[15%]" },
    ],
    [t],
  )

  const itemRenderer = useMemo(() => getItemRenderer(t, user?.id), [t, user?.id])

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
