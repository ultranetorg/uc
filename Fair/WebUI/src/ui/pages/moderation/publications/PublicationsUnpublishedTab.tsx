import { ReactNode, useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { TFunction } from "i18next"
import { isNumber } from "lodash"

import { useStoreRolesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetUnpublishedPublications } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { PublicationUnpublished } from "types"
import { Pagination, Table, TableColumn, TableEmptyState, TableItem } from "ui/components"
import { renderAuthor2, renderPublication2, renderWithYouBadge } from "ui/renderers/utils"
import { parseInteger, routes } from "utils"

const getItemRenderer =
  (t: TFunction, publisherIds?: string[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publication = item as PublicationUnpublished

    switch (column.type) {
      case "publication":
        return renderPublication2(publication.title, publication.logoId)
      case "author":
        return renderWithYouBadge(
          t,
          renderAuthor2(publication.authorTitle, publication.authorLogoId),
          !!publisherIds?.includes(publication.authorId),
        )
    }

    return undefined
  }

export const PublicationsUnpublishedTab = () => {
  const { t } = useTranslation("publicationsUnpublishedTab")
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

  const { data: publications } = useGetUnpublishedPublications(storeId, page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
      : 0

  const columns = useMemo(
    () => [
      { accessor: "publication", label: t("columns:publication"), type: "publication", className: "w-2/4" },
      { accessor: "type", label: t("columns:type"), type: "type", className: "w-1/4 capitalize" },
      { accessor: "author", label: t("columns:author"), type: "author", className: "w-1/4" },
    ],
    [t],
  )

  const itemRenderer = useMemo(() => getItemRenderer(t, publisherIds), [t, publisherIds])

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.unpublishedPublication(storeId!, id)),
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
