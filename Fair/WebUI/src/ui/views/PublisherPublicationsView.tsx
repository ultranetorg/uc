import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublisherPublications } from "entities"
import { AuthorDetails } from "types"
import { Pagination } from "ui/components"
import { AuthorProfileProps } from "ui/components/author"
import { PublicationsTable, ProductStoresModal } from "ui/components/specific"

type PublisherPublicationsViewBaseProps = {
  siteId: string
  author?: AuthorDetails
  isModalOpen: boolean
  onModalOpenChange: (open: boolean) => void
}

export type PublisherPublicationsViewProps = Pick<AuthorProfileProps, "size"> & PublisherPublicationsViewBaseProps

export const PublisherPublicationsView = memo(
  ({ siteId, author, isModalOpen, onModalOpenChange }: PublisherPublicationsViewProps) => {
    const { t } = useTranslation()

    const [page, setPage] = useState(0)
    const [selectedPublicationId, setPublicationId] = useState<string | undefined>()

    const { data: publications } = useGetPublisherPublications(siteId, author?.id, page, DEFAULT_PAGE_SIZE_20)

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
      return <>Loading </>
    }

    if (!publications.items.length) return null

    return (
      <>
        <>
          <div className="flex items-center justify-between">
            <span className="text-lg font-semibold leading-10">
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
