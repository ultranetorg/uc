import { useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetSiteUsers } from "entities"
import { useResolveSiteId, useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger, routes } from "utils"

import { usersTabItemRenderer } from "./usersTabItemRenderer"

export const UsersTab = () => {
  const navigate = useNavigate()
  const siteId = useResolveSiteId()
  const { t } = useTranslation("usersTab")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })
  const [page, setPage] = useState(state.page)

  const { data: users, isPending } = useGetSiteUsers(siteId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(() => [{ accessor: "user", label: t("common:user"), type: "user" }], [t])

  const handleRowClick = useCallback((id: string) => navigate(routes.moderation.user(siteId!, id)), [navigate, siteId])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  if (!users || isPending) return <>LOADING</>

  return (
    <>
      <Table
        columns={columns}
        items={users?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={usersTabItemRenderer}
        onRowClick={handleRowClick}
        emptyState={<TableEmptyState message={t("noUsers")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </>
  )
}
