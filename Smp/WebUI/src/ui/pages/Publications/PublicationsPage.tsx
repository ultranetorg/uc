import { ChangeEvent, useCallback } from "react"
import { Link } from "react-router-dom"

import { useSearchPublications } from "entities"
import { PublicationCard, SearchInput } from "ui/components"

import { useQueryParams } from "./hooks"

export const PublicationsPage = () => {
  const { setSearchParams, name, page, pageSize } = useQueryParams()

  const { isPending, data: publications } = useSearchPublications(name ?? undefined, page, pageSize)

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
    <>
      <SearchInput
        value={name ?? ""}
        onChange={handleChange}
        className="mb-4 h-10 w-full text-black"
        placeholder="Enter Product Name"
      />
      <div className="mb-4 text-center text-purple-500">(Advertisements)</div>
      {isPending ? (
        <h2>Loading</h2>
      ) : publications?.items !== undefined ? (
        <div className="flex w-full flex-wrap gap-x-6 gap-y-6">
          {publications.items.map(p => (
            <Link to={`/p/${p.id}`} key={p.id}>
              <PublicationCard publicationName={p.productName} authorTitle={p.authorTitle} />
            </Link>
          ))}
        </div>
      ) : (
        <h2>Not found</h2>
      )}
    </>
  )
}
