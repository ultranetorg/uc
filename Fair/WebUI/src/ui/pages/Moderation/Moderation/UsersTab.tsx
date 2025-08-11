import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { isNumber } from "lodash"
import { useTranslation } from "react-i18next"

import { DEFAULT_PAGE_SIZE_2 } from "config"
import { useGetUserProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger } from "utils"

export const UsersTab = () => {
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

  const { data: users } = useGetUserProposals(siteId, page, DEFAULT_PAGE_SIZE_2)
  console.log(users)
  const pagesCount = users?.totalItems && users.totalItems > 0 ? Math.ceil(users.totalItems / DEFAULT_PAGE_SIZE_2) : 0

  const columns = useMemo(
    () => [
      { accessor: "creator", label: t("reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("text"), type: "text", className: "w-[23%]" },
      { accessor: "rating", label: t("common:rating"), type: "rating", className: "w-[5%]" },
      { accessor: "creationTime", label: t("common:date"), type: "date", className: "w-[8%]" },
      { accessor: "action", label: t("common:action"), type: "approve-reject-action", className: "w-[17%]" },
    ],
    [t],
  )

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
        //itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noUsers")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
