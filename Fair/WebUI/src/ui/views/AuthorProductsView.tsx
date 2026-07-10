import { memo, useState } from "react"
import { useTranslation } from "react-i18next"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetAuthorProducts } from "entities"
import { Pagination } from "ui/components"
import { ProductsTable } from "ui/components/specific"

export type AuthorProductsViewProps = {
  authorId: string
  onProductStoresClick: (id: string) => void
}

export const AuthorProductsView = memo(({ authorId, onProductStoresClick }: AuthorProductsViewProps) => {
  const { t } = useTranslation()

  const [page, setPage] = useState(0)
  const { data: products, isPending } = useGetAuthorProducts(authorId, page, DEFAULT_PAGE_SIZE_20)

  const pagesCount =
    products?.totalItems && products.totalItems > 0 ? Math.ceil(products.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  if (!products || isPending) {
    return <>Loading </>
  }

  return (
    <>
      <div className="flex items-center justify-between">
        <span className="text-lg font-semibold leading-10">
          {products?.totalItems} {t("common:publications")}
        </span>
        <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
      </div>
      <ProductsTable items={products.items} onProductStoresClick={onProductStoresClick} />
      <div className="flex justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
      </div>
    </>
  )
})
