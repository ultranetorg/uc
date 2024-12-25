import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Navigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetResource, useGetResourceInboundLinks, useGetResourceLinks } from "entities"
import { usePageId } from "hooks"
import { PageLoader, List, ListRow, TableItem, Table, TableFooter, TableColumn } from "ui/components"
import { getListItemRenderer, getTableItemRenderer } from "ui/renderers"

import { getListRows, getTableColumns } from "./constants"
import { DEFAULT_PAGE_SIZE } from "constants"

export const ResourcePage = () => {
  const { author } = useParams()
  const params = useParams()
  useDocumentTitle(
    author && params["*"] ? `Resource - ${author}/${params["*"]} | Ultranet Explorer` : "Resource | Ultranet Explorer",
  )

  const pageId = usePageId()
  const { t } = useTranslation(pageId)

  const [linksPage, setLinksPage] = useState(0)
  const [inboundLinksPage, setInboundLinksPage] = useState(0)

  const { data: resource, isLoading } = useGetResource(author, params["*"])
  const { data: links, isLoading: isLinksLoading } = useGetResourceLinks(author!, params["*"]!, linksPage)
  const { data: inboundLinks, isLoading: isInboundLinksLoading } = useGetResourceInboundLinks(
    author!,
    params["*"]!,
    linksPage,
  )

  const [linksItems, setLinksItems] = useState<TableItem[] | undefined>()
  const [inboundLinksItems, setInboundLinksItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])
  const columns: TableColumn[] = useMemo(() => getTableColumns(t), [t])

  const listItemRenderer = useMemo(() => getListItemRenderer(), [])
  const tableItemRenderer = useMemo(() => getTableItemRenderer("resources"), [])

  useEffect(() => {
    if (isLoading || isLinksLoading || resource?.inbounds.totalItems === 0) {
      setLinksItems([])
      return
    }

    const items = linksPage > 0 && resource ? links?.items : resource?.inbounds.items
    const mapped = items?.map(x => ({ id: x.address.author + x.address.resource, ...x }))
    setLinksItems(mapped)
  }, [isLinksLoading, isLoading, links?.items, linksPage, resource])

  useEffect(() => {
    if (isLoading || isInboundLinksLoading || resource?.outbounds.totalItems === 0) {
      setInboundLinksItems([])
      return
    }

    const items = inboundLinksPage > 0 && resource ? inboundLinks?.items : resource?.outbounds.items
    const mapped = items?.map(x => ({ id: x.address.author + x.address.resource, ...x }))
    setInboundLinksItems(mapped)
  }, [isInboundLinksLoading, isLoading, inboundLinks?.items, inboundLinksPage, resource])

  if (isLoading) {
    return <PageLoader />
  }

  if (!resource) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={resource} rows={rows} itemRenderer={listItemRenderer} />
      {linksItems && !!linksItems.length && (
        <Table
          title={t("inbounds")}
          itemRenderer={tableItemRenderer}
          columns={columns}
          items={linksItems}
          footer={
            <TableFooter
              pageSize={DEFAULT_PAGE_SIZE}
              page={linksPage}
              onPageChange={setLinksPage}
              totalItems={resource.inbounds.totalItems}
            />
          }
        />
      )}
      {inboundLinksItems && !!inboundLinksItems.length && (
        <Table
          title={t("outbounds")}
          itemRenderer={tableItemRenderer}
          columns={columns}
          items={inboundLinksItems}
          footer={
            <TableFooter
              pageSize={DEFAULT_PAGE_SIZE}
              page={inboundLinksPage}
              onPageChange={setInboundLinksPage}
              totalItems={resource.outbounds.totalItems}
            />
          }
        />
      )}
    </div>
  )
}
