import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { useGetAuthor } from "entities"
import { useParams, useSiteTitle } from "hooks"
import { AuthorProfile } from "ui/components/author"
import { ProductStoresModal } from "ui/components/specific"
import { AuthorProductsView } from "ui/views"

export const AuthorPage = () => {
  const { authorId } = useParams()
  const { t } = useTranslation("")

  const [selectedProductId, setSelectedProductId] = useState<string | undefined>()

  const { isPending, data: author, error } = useGetAuthor(authorId)
  if (error) throw error

  useSiteTitle(author?.title ? `Author - ${author?.title}` : undefined)

  const handleProductStoresClick = useCallback((id: string) => setSelectedProductId(id), [])

  const handleModalClose = useCallback(() => setSelectedProductId(undefined), [])

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <>
      <AuthorProfile t={t} size="compact" author={author} />
      <AuthorProductsView authorId={author.id} onProductStoresClick={handleProductStoresClick} />
      {selectedProductId && <ProductStoresModal onClose={handleModalClose} productId={selectedProductId!} />}
    </>
  )
}
