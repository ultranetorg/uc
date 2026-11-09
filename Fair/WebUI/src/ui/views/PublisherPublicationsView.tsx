import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublisherPublications } from "entities"
import { AuthorDetails } from "types"
import { Pagination, TableSectionSkeleton } from "ui/components"
import { PublicationsTable, ProductStoresModal } from "ui/components/specific"

export type PublisherPublicationsViewProps = {
  storeId: string
  author?: AuthorDetails
  isModalOpen: boolean
  onModalOpenChange: (open: boolean) => void
}

export const PublisherPublicationsView = memo(
  ({ storeId, author, isModalOpen, onModalOpenChange }: PublisherPublicationsViewProps) => {
    const { t } = useTranslation()

    const [page, setPage] = useState(0)
    const [selectedPublicationId, setPublicationId] = useState<string | undefined>()

    const { data: publications } = useGetPublisherPublications(storeId, author?.id, page, DEFAULT_PAGE_SIZE_20)

    const pagesCount =
      publications?.totalItems && publications.totalItems > 0
        ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
        : 0

    const handleModalClose = useCallback(() => onModalOpenChange(false), [onModalOpenChange])

    const handleProductStoreClick = useCallback(
      (id: string) => {
        onModalOpenChange(true)
        setPublicationId(id)
      },
      [onModalOpenChange],
    )

    if (!author || !publications) {
      return <TableSectionSkeleton withCategory />
    }

    if (!publications.items.length) return null

    return (
      <>
        <>
          <div className="flex items-center justify-between">
            <span className="text-lg leading-10 font-semibold">
              {publications?.totalItems} {t("common:publications")}
            </span>
            <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
          </div>
          <PublicationsTable
            className="flex flex-col rounded-lg border border-gray-300 bg-gray-100"
            items={publications.items}
            onProductStoresClick={handleProductStoreClick}
          />
          <div className="flex justify-end">
            <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
          </div>
        </>
        {isModalOpen && <ProductStoresModal onClose={handleModalClose} productId={selectedPublicationId!} />}
      </>
    )
  },
)
