import { useCallback, useEffect, useMemo } from "react"
import { Link, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendums } from "entities"
import { Input, Pagination, Select, SelectItem, Table } from "ui/components"
import { GovernanceModerationHeader } from "ui/components/specific"
import { TableEmptyState } from "ui/components/referendums"
import { usePagePagination } from "ui/pages/hooks"
import { getReferendumsItemRenderer as getItemRenderer } from "ui/renderers"

export const ReferendumsPage = () => {
  const { page, setPage, pageSize, setPageSize, search, setSearch } = usePagePagination()
  const { siteId } = useParams()
  const { t } = useTranslation("referendums")

  const itemRenderer = useMemo(() => getItemRenderer(t), [t])

  const { isPending, data: referendums } = useGetAuthorReferendums(siteId, page, pageSize, search)
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
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader
        siteId={siteId!}
        title={t("title")}
        onCreateButtonClick={() => console.log("GovernanceModerationHeader")}
        homeLabel={t("common:home")}
        createButtonLabel={t("createReferendum")}
      />
      <div className="flex flex-col justify-between gap-4 xl:flex-row">
        <Input
          placeholder={t("searchReferendum")}
          value={search}
          onChange={setSearch}
          id="referendums-search-input"
          className="w-full max-w-120"
        />
        <Pagination onPageChange={() => console.log("onPageChange")} page={2} pagesCount={10} />
      </div>
      <Table
        columns={[
          { accessor: "text", label: "Text", className: "w-[26%]" },
          { accessor: "createdBy", label: "Created By", className: "w-[18%]" },
          { accessor: "expiration", label: "Days Left", className: "w-[7%]" },
          { accessor: "action", label: "Action", type: "proposal", className: "w-[26%]" },
          { accessor: "voting", label: "Current voting", type: "voting", className: "w-[11%]" },
        ]}
        items={referendums?.items}
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noReferendums")} />}
      />
      <div className="flex justify-end">
        <Pagination onPageChange={() => console.log("onPageChange")} page={2} pagesCount={10} />
      </div>
    </div>
  )

  // return (
  //   <div className="my-3 flex flex-col gap-3">
  //     <div className="flex w-80 gap-3">
  //       <Input placeholder="Search site" value={search} onChange={setSearch} />
  //       <Select items={pageSizes} value={pageSize} onChange={handlePageSizeChange} />
  //       <Pagination pagesCount={pagesCount} onPageChange={setPage} page={page} />
  //     </div>
  //     <div>
  //       <table style={{ width: "100%", borderCollapse: "collapse" }}>
  //         <thead>
  //           <tr>
  //             <th>ID</th>
  //             <th>Text</th>
  //             <th>Expiration</th>
  //             <th>Votes</th>
  //             <th>Type</th>
  //             <th>Proposal</th>
  //           </tr>
  //         </thead>
  //         <tbody>
  //           {referendums?.items?.map(r => (
  //             <tr key={r.id}>
  //               <td>
  //                 <Link to={`/${siteId}/a-r/${r.id}`}>{r.id}</Link>
  //               </td>
  //               <td>
  //                 <div>{r.text}</div>
  //               </td>
  //               <td>{r.expiration}</td>
  //               <td>
  //                 <span className="text-red-500">{r.yesCount}</span> /{" "}
  //                 <span className="text-green-500">{r.noCount}</span> /{" "}
  //                 <span className="text-gray-500">{r.absCount}</span>
  //               </td>
  //               <td>{t(r.proposal.$type, { ns: "votableOperations" })}</td>
  //               <td>{JSON.stringify(r.proposal)}</td>
  //             </tr>
  //           ))}
  //         </tbody>
  //       </table>
  //     </div>
  //   </div>
  // )
}
