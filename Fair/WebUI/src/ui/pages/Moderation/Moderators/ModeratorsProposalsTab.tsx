import { useNavigate, useParams } from "react-router-dom"
import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useGetModeratorProposals } from "entities"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger } from "utils"

import { getModeratorsProposalsItemRenderer } from "./moderatorProposalsItemRenderer"

export const ModeratorsProposalsTab = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()
  const { t } = useTranslation("moderatorsProposalsTab")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: proposals } = useGetModeratorProposals(siteId, "", page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      {
        accessor: "moderators",
        label: t("common:moderators"),
        type: "accounts-list",
        className: "w-[30%] first-letter:uppercase",
      },
      { accessor: "by", label: t("common:createdBy"), type: "account", className: "w-[12%]" },
      { accessor: "action", label: t("common:action"), type: "action-short", className: "w-[12%]" },
      { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for", className: "w-[12%]" },
      { accessor: "votes", label: t("common:votes"), type: "votes", className: "capitalize w-[12%]" },
      { accessor: "nabb", label: t("common:nabb"), type: "nabb", title: t("common:nabbFull"), className: "w-[12%]" },
    ],
    [t],
  )
  const itemRenderer = useMemo(() => getModeratorsProposalsItemRenderer(t), [t])

  const handleTableRowClick = useCallback((id: string) => navigate(`/${siteId}/m/m/p/${id}`), [navigate, siteId])

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
        items={proposals?.items}
        tableBodyClassName="text-2sm leading-5"
        itemRenderer={itemRenderer}
        emptyState={<TableEmptyState message={t("noProposals")} />}
        onRowClick={handleTableRowClick}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={page} />
      </div>
    </div>
  )
}

//   "items": [
//     {
//       "moderators": [
//         {
//           "id": "340582-0",
//           "nickname": "f258",
//           "address": "qqp934dy7amhnwdndqmgrt2sdde5xn8j0yy9tk7x2clm6yrpldfqqqdj8h8btes"
//         }
//       ],
//       "id": "340582-179",
//       "operation": "site-moderator-addition",
//       "optionsVotesCount": [
//         0
//       ],
//       "neitherCount": 0,
//       "anyCount": 0,
//       "banCount": 0,
//       "banishCount": 0,
//       "creationTime": 2699,
//       "title": "Add new moderator",
//       "text": null,
//       "by": {
//         "id": "406118-2",
//         "nickname": "f262",
//         "address": "qqpx9rzunpekud06wcahnu0aw8txnzyxra4wk7jn6udrmkkpfdwsqnydv7xbtes"
//       },
//       "multipleOptions": false,
//       "hoursLeft": -1
//     },
//     {
//       "moderators": null,
//       "id": "340582-178",
//       "operation": "site-moderator-addition",
//       "optionsVotesCount": [
//         0,
//         0
//       ],
//       "neitherCount": 0,
//       "anyCount": 0,
//       "banCount": 0,
//       "banishCount": 0,
//       "creationTime": 2699,
//       "title": "Add new moderator",
//       "text": null,
//       "by": {
//         "id": "406118-2",
//         "nickname": "f262",
//         "address": "qqpx9rzunpekud06wcahnu0aw8txnzyxra4wk7jn6udrmkkpfdwsqnydv7xbtes"
//       },
//       "multipleOptions": true,
//       "hoursLeft": -1
//     }
//   ],
//   "totalItems": 2,
//   "page": 0,
//   "pageSize": 20
// }
