import { useCallback, useEffect } from "react"
import { Link, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { PAGE_SIZES } from "constants"
import { useGetAuthorReferendums } from "entities"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const AuthorReferendumsPage = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch } = usePagePagination()

  const { siteId } = useParams()
  const { isPending, data: referendums } = useGetAuthorReferendums(siteId, page, pageSize, search)

  const { t } = useTranslation()

  const pagesCount =
    referendums?.totalItems && referendums.totalItems > 0 ? Math.ceil(referendums.totalItems / pageSize) : 0

  useEffect(() => {
    if (!isPending && pagesCount > 0 && page > pagesCount) {
      setPage(0)
    }
  }, [isPending, page, pagesCount, setPage])

  const handlePageSizeChange = useCallback(
    (value: string) => {
      setPage(0)
      setPageSize(parseInt(value))
    },
    [setPage, setPageSize],
  )

  return (
    <div className="my-3 flex flex-col gap-3">
      <div className="flex w-80 gap-3">
        <Input placeholder="Search site" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onClick={setPage} page={page} />
      </div>
      <div>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr>
              <th>ID</th>
              <th>Text</th>
              <th>Expiration</th>
              <th>Votes</th>
              <th>Type</th>
              <th>Proposal</th>
            </tr>
          </thead>
          <tbody>
            {referendums?.items?.map(r => (
              <tr key={r.id}>
                <td>
                  <Link to={`/${siteId}/a-r/${r.id}`}>{r.id}</Link>
                </td>
                <td>
                  <div>{r.text}</div>
                </td>
                <td>{r.expiration}</td>
                <td>
                  <span className="text-red-500">{r.yesCount}</span> /{" "}
                  <span className="text-green-500">{r.noCount}</span> /{" "}
                  <span className="text-gray-500">{r.absCount}</span>
                </td>
                <td>{t(r.proposal.$type, { ns: "votableOperations" })}</td>
                <td>{JSON.stringify(r.proposal)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
