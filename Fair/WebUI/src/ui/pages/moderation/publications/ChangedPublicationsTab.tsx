import { ReactNode, useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreRolesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetChangedPublications } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { PublicationChanged } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderAuthor2, renderCategory, renderPublication2, renderWithYouBadge } from "ui/renderers/utils"
import { parseInteger, routes } from "utils"

const getChangedPublicationItemRenderer =
  (t: TFunction, publisherIds?: string[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publication = item as PublicationChanged

    switch (column.type) {
      case "account":
        return renderWithYouBadge(
          t,
          renderAuthor2(publication.authorTitle, publication.authorLogoId),
          !!publisherIds?.includes(publication.authorId),
        )
      case "category":
        return renderCategory(publication.categoryTitle)
      case "publication":
        return renderPublication2(publication.title, publication.logoId)
      case "version":
        return publication[column.accessor as "currentVersion" | "latestVersion"]
    }

    return undefined
  }

export const ChangedPublicationsTab = () => {
  const { t } = useTranslation("tabChangedPublications")
  const navigate = useNavigate()

  const storeId = useResolveStoreId()
  const { publisherIds } = useStoreRolesContext()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: publications } = useGetChangedPublications(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      {
        accessor: "publication",
        label: t("columns:product"),
        type: "publication",
        className: "w-[30%]",
      },
      { accessor: "author", label: t("columns:author"), type: "account", className: "w-[22%]" },
      {
        accessor: "category",
        label: t("columns:category"),
        type: "category",
        className: "w-[18%]",
      },
      {
        accessor: "currentVersion",
        label: t("columns:currentVersion"),
        type: "version",
        className: "w-[15%] text-right",
      },
      {
        accessor: "latestVersion",
        label: t("columns:latestVersion"),
        type: "version",
        className: "w-[15%] text-right",
      },
    ],
    [t],
  )

  const itemRenderer = useMemo(() => getChangedPublicationItemRenderer(t, publisherIds), [t, publisherIds])

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.changedPublication(storeId!, id)),
    [navigate, storeId],
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
        items={publications?.items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noPublications")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}
