import { ChangeEvent, useCallback } from "react"

import { useGetProducts } from "entities"
import { SearchInput } from "ui/components"

import { useQueryParams } from "./hooks"

export const ProductsPage = () => {
  const { setSearchParams, name, page, pageSize } = useQueryParams()

  const { isPending, data: products } = useGetProducts(name ?? undefined, page, pageSize)

  const handleChange = useCallback(
    (e: ChangeEvent<HTMLInputElement>) => {
      const params = {
        ...(e.target.value !== "" && { name: e.target.value }),
        ...(page !== undefined && { page: page.toString() }),
        ...(pageSize !== undefined && { pageSize: pageSize.toString() }),
      }
      setSearchParams(params)
    },
    [page, pageSize, setSearchParams],
  )

  return (
    <div>
      <h1>ProductsPage</h1>
      <SearchInput value={name ?? ""} onChange={handleChange} />
      {isPending ? (
        <h2>Loading</h2>
      ) : products?.items !== undefined ? (
        products.items.map(p => <div key={p.id}>{JSON.stringify(p)}</div>)
      ) : (
        <h2>Not found</h2>
      )}
    </div>
  )
}
