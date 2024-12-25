import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams, Navigate } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { useGetAuthor, useGetAuthorResources } from "entities"
import { usePageId } from "hooks"
import { List, ListRow, PageLoader, TableItem, ResourcesTable } from "ui/components"
import { getListItemRenderer } from "ui/renderers"

import { getListRows } from "./constants"

export const AuthorPage = () => {
  const { authorId } = useParams()
  useDocumentTitle(authorId ? `Author - ${authorId} | Ultranet Explorer` : "Author | Ultranet Explorer")

  const pageId = usePageId()!
  const { t } = useTranslation(pageId)
  const { currency } = useSettings()

  const [page, setPage] = useState(0)

  const { data: author, isLoading } = useGetAuthor(authorId)
  const { data: resources, isLoading: isResourcesLoading } = useGetAuthorResources(author?.name, page)

  const [items, setItems] = useState<TableItem[] | undefined>()

  const rows: ListRow[] = useMemo(() => getListRows(t), [t])

  const listItemRenderer = useMemo(
    () => getListItemRenderer(currency, author?.rateUsd, author?.emissionMultiplier),
    [currency, author?.rateUsd, author?.emissionMultiplier],
  )

  useEffect(() => {
    if (isLoading || isResourcesLoading || author!.resources.totalItems === 0) {
      setItems([])
      return
    }

    const resourceItems = page > 0 && resources ? resources.items : author!.resources.items
    const items = resourceItems.map((item, index) => ({ ...item, id: index.toString(), author: author?.name }))
    setItems(items)
  }, [author, currency, resources, isLoading, isResourcesLoading, page])

  if (isLoading) {
    return <PageLoader />
  }

  if (!author) {
    return <Navigate to={"/"} />
  }

  return (
    <div className="flex flex-col gap-7 leading-[1.125rem]">
      <List items={author} rows={rows} itemRenderer={listItemRenderer} />
      {items && !!items.length && (
        <ResourcesTable totalItems={author.resources.totalItems} items={items} page={page} onPageChange={setPage} />
      )}
    </div>
  )
}
