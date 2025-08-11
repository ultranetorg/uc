import { useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { DEFAULT_PAGE_SIZE_2 } from "config"
import { useGetPublicationProposals } from "entities"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger } from "utils"

export const PublicationsTab = () => {
  const { t } = useTranslation("tabPublications")
  const { siteId } = useParams()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: publications } = useGetPublicationProposals(siteId, page, DEFAULT_PAGE_SIZE_2)
  const pagesCount =
    publications?.totalItems && publications.totalItems > 0
      ? Math.ceil(publications.totalItems / DEFAULT_PAGE_SIZE_2)
      : 0

  const columns = useMemo(
    () => [
      { accessor: "creator", label: t("reviewer"), type: "account", className: "w-[15%]" },
      { accessor: "publication", label: t("common:publication"), type: "publication", className: "w-[17%]" },
      { accessor: "text", label: t("text"), type: "text", className: "w-[23%]" },
      { accessor: "rating", label: t("common:rating"), type: "rating", className: "w-[5%]" },
      { accessor: "creationTime", label: t("common:date"), type: "date", className: "w-[8%]" },
      { accessor: "action", label: t("common:action"), type: "approve-reject-action", className: "w-[17%]" },
    ],
    [t],
  )

  const handlePageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  return (
    <div className="flex flex-col gap-6">
      <Table
        columns={columns}
        items={publications?.items}
        //itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noPublications")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )

  // return (
  //   <div className="flex flex-col">
  //     <div className="flex justify-between">
  //       <span>Publications</span>
  //       <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
  //     </div>
  //     <div>
  //       <table style={{ width: "100%", borderCollapse: "collapse" }}>
  //         <thead>
  //           <tr>
  //             <th>Id</th>
  //             <th>Category</th>
  //             <th>Author</th>
  //             <th>Product</th>
  //             <th>Status</th>
  //           </tr>
  //         </thead>
  //         <tbody>
  //           {isError ? (
  //             <tr>
  //               <td>Unable to load</td>
  //             </tr>
  //           ) : isPending || !publications ? (
  //             <tr>
  //               <td>Loading...</td>
  //             </tr>
  //           ) : publications.items.length === 0 ? (
  //             <tr>
  //               <td>No publications</td>
  //             </tr>
  //           ) : (
  //             publications.items.map(p => (
  //               <tr key={p.id}>
  //                 <td>
  //                   <Link to={`/${siteId}/m-p/${p.id}`}> {p.id}</Link>
  //                 </td>
  //                 <td>{p.categoryId}</td>
  //                 <td>{p.authorId}</td>
  //                 <td>{p.productId}</td>
  //                 <td></td>
  //               </tr>
  //             ))
  //           )}
  //         </tbody>
  //       </table>
  //     </div>
  //   </div>
  // )
}
