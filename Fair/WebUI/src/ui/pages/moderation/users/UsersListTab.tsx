import { useCallback, useMemo, useState, ReactNode } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetStoreUsers } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { User } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderUser, renderWithYouBadge } from "ui/renderers"
import { parseInteger, routes } from "utils"

const getUsersListItemRenderer =
  (t: TFunction, currentUserId?: string) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const user = item as User

    switch (column.type) {
      case "user":
        return renderWithYouBadge(t, renderUser(user), user.id === currentUserId)
    }

    return null
  }

export const UsersListTab = () => {
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const { t } = useTranslation("usersListTab")
  const { user } = useUserContext()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })
  const [page, setPage] = useState(state.page)

  const { data: users, isPending } = useGetStoreUsers(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(() => [{ accessor: "user", label: t("columns:user"), type: "user" }], [t])

  const itemRenderer = useMemo(() => getUsersListItemRenderer(t, user?.id), [t, user?.id])

  const handleRowClick = useCallback(
    (id: string) => navigate(routes.moderation.user(storeId!, id)),
    [navigate, storeId],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  if (!users || isPending) return <div>Loading{import.meta.env.DEV ? " (UsersListTab)" : ""}</div>

  return (
    <>
      <Table
        columns={columns}
        items={users?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        onRowClick={handleRowClick}
        emptyState={<TableEmptyState message={t("noUsers")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </>
  )
}
