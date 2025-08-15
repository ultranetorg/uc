import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { isNumber } from "lodash"
import { useTranslation } from "react-i18next"

import { DEFAULT_PAGE_SIZE_20 as PAGE_SIZE_20 } from "config"
import { useGetUserProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { getUsersItemRenderer } from "ui/renderers"
import { parseInteger } from "utils"
import { getCommonColumns } from "./constants"

export const UserRegistrationsTab = () => {
  const { t } = useTranslation("tabUsers")
  const { siteId } = useParams()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })
  const [page, setPage] = useState(state.page)

  const { data: users } = useGetUserProposals(siteId, page, PAGE_SIZE_20)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      { accessor: "title", label: t("common:title"), type: "title", className: "w-[23%]" },
      { accessor: "signer", label: t("common:user"), type: "account", className: "w-[15%]" },
      ...getCommonColumns(t),
    ],
    [t],
  )
  const itemRenderer = useMemo(() => getUsersItemRenderer(t), [t])

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={users?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noUsers")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
