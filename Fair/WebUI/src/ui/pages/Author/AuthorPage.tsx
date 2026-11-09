import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useGetAuthor } from "entities"
import { useParams, useStoreTitle } from "hooks"
import { AuthorProfile } from "ui/components/author"
import { ProductStoresModal } from "ui/components/specific"
import { AuthorProductsView } from "ui/views"

import { AuthorPageSkeleton } from "./AuthorPage.Skeleton"

export const AuthorPage = () => {
  const { authorId } = useParams()
  const { t } = useTranslation("")

  const [selectedProductId, setSelectedProductId] = useState<string | undefined>()

  const { isLoading, data: author, error } = useGetAuthor(authorId)
  if (error) throw error

  useStoreTitle(author?.title ? `Author - ${author?.title}` : undefined)

  const handleProductStoresClick = useCallback((id: string) => setSelectedProductId(id), [])

  const handleModalClose = useCallback(() => setSelectedProductId(undefined), [])

  if (isLoading || !author) {
    return <AuthorPageSkeleton />
  }

  return (
    <>
      <AuthorProfile t={t} author={author} showStoreInfo={true} />
      <AuthorProductsView authorId={author.id} onProductStoresClick={handleProductStoresClick} />
      {selectedProductId && <ProductStoresModal onClose={handleModalClose} productId={selectedProductId!} />}
    </>
  )
}
