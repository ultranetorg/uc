import { useCallback, useEffect } from "react"
import { Link, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { PAGE_SIZES } from "config"
import { useGetModeratorDiscussions } from "entities"
import { Input, Pagination, Select, SelectItem } from "ui/components"
import { usePagePagination } from "ui/pages/hooks"

const pageSizes: SelectItem[] = PAGE_SIZES.map(x => ({ label: x.toString(), value: x.toString() }))

export const ModeratorDiscussionsPage = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch } = usePagePagination()

  const { siteId } = useParams()
  const { t } = useTranslation()
  const { isPending, data: discussions } = useGetModeratorDiscussions(siteId, page, pageSize, search)

  const pagesCount =
    discussions?.totalItems && discussions.totalItems > 0 ? Math.ceil(discussions.totalItems / pageSize) : 0

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
    <div>
      <div className="flex w-80 gap-3">
        <Input placeholder="Search site" value={search} onChange={setSearch} />
        <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
        <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
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
              <th>Option</th>
              <th>Comments Count</th>
            </tr>
          </thead>
          <tbody>
            {discussions?.items?.map(d => (
              <tr key={d.id}>
                <td>
                  <Link to={`/${siteId}/m-d/${d.id}`}>{d.id}</Link>
                </td>
                <td>
                  <div>{d.text}</div>
                </td>
                <td>{d.expiration}</td>
                <td>
                  <span className="text-red-500">{d.yesCount}</span> /{" "}
                  <span className="text-green-500">{d.noCount}</span> /{" "}
                  <span className="text-gray-500">{d.absCount}</span>
                </td>
                <td>{t(d.option.$type, { ns: "votableOperations" })}</td>
                <td>{JSON.stringify(d.option)}</td>
                <td>{d.commentsCount}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
