import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useSiteRolesContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetAuthorPublications } from "entities"
import { AuthorDetails } from "types"
import { Pagination } from "ui/components"
import { AuthorProfile, AuthorProfileProps } from "ui/components/author"
import { PublicationsTable, PublicationStoresModal } from "ui/components/specific"

type AuthorPublicationsViewBaseProps = {
  siteId: string
  author?: AuthorDetails
  isModalOpen: boolean
  onModalOpenChange: (open: boolean) => void
}

export type AuthorPublicationsViewProps = Pick<AuthorProfileProps, "size"> & AuthorPublicationsViewBaseProps

export const AuthorPublicationsView = memo(
  ({ size, siteId, author, isModalOpen, onModalOpenChange }: AuthorPublicationsViewProps) => {
    const { isPublisher, isModerator } = useSiteRolesContext()
    const { t } = useTranslation("authorPublicationsView")

    const [page, setPage] = useState(0)
    const [selectedPublicationId, setPublicationId] = useState<string | undefined>()

    const { data: publications } = useGetAuthorPublications(siteId, author?.id, page, DEFAULT_PAGE_SIZE_20)

    const pagesCount =
      publications?.totalItems && publications.totalItems > 0
        ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_20)
        : 0

    const handleModalClose = useCallback(() => onModalOpenChange(false), [onModalOpenChange])

    const handlePublicationStoreClick = useCallback(
      (id: string) => {
        onModalOpenChange(true)
        setPublicationId(id)
      },
      [onModalOpenChange],
    )

    if (!author || !publications) {
      return <>LOADING ⏱️</>
    }

    return (
      <>
        <AuthorProfile t={t} size={size} author={author} isPublisher={isPublisher} isModerator={isModerator} />
        {publications.items.length && (
          <>
            <div className="flex items-center justify-between">
              <span className="text-3.5xl font-semibold leading-10">
                {publications?.totalItems} {t("common:products")}
              </span>
              <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
            </div>
            <PublicationsTable
              className="flex flex-col rounded-lg border border-gray-300 bg-gray-100"
              items={publications.items}
              onPublicationStoresClick={handlePublicationStoreClick}
            />
            <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
          </>
        )}
        {isModalOpen && <PublicationStoresModal onClose={handleModalClose} publicationId={selectedPublicationId!} />}
      </>
    )
  },
)
