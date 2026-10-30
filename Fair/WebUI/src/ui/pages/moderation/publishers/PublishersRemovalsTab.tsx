import { useCallback, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetPublisherProposals } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger, routes } from "utils"

import { getPublisherProposalsItemRenderer } from "./publisherProposalsItemRenderer"

export const PublishersRemovalsTab = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, publisherIds } = useStoreRolesContext()
  const { user } = useUserContext()
  const { t } = useTranslation("publishersProposalsTab")

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: proposals } = useGetPublisherProposals(storeId, "", page, DEFAULT_PAGE_SIZE_20)
  const pagesCount =
    proposals?.totalItems && proposals.totalItems > 0 ? Math.ceil(proposals.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      {
        accessor: "publishers",
        label: t("columns:publishers"),
        type: "accounts-list",
        className: "w-[36%]",
      },
      { accessor: "by", label: t("columns:createdBy"), type: "account", className: "w-[16%]" },
      {
        accessor: "progress",
        label: t("columns:progress.label"),
        title: t("columns:progress.proposalTitle"),
        type: "progress",
        className: "w-[18%]",
      },
      { accessor: "yourVote", label: t("columns:yourVote"), type: "your-vote", className: "w-[16%]" },
      { accessor: "lastsFor", label: t("columns:openFor"), type: "lasts-for", className: "w-[14%]" },
    ],
    [t],
  )

  const moderatorId = isModerator ? user?.id : undefined
  const itemRenderer = useMemo(
    () => getPublisherProposalsItemRenderer(t, { store, policies, moderatorId, publisherIds }),
    [t, store, policies, moderatorId, publisherIds],
  )

  const handleTableRowClick = useCallback(
    (id: string) => navigate(routes.moderation.publisherRemoval(storeId!, id)),
    [navigate, storeId],
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
