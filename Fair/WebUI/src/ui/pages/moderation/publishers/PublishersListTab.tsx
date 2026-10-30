import { ReactNode, useCallback, useEffect, useMemo, useState } from "react"
import { Link } from "react-router-dom"
import { TFunction } from "i18next"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"
import { isNumber, truncate } from "lodash"

import { useStorePoliciesContext, useStoreRolesContext } from "app"
import { SvgCheckCircleFill, SvgSearchMd, SvgX } from "assets"
import { DEFAULT_PAGE_SIZE_20, SEARCH_DELAY } from "config"
import { useGetStorePublishers } from "entities"
import { useResolveStoreId, useUrlParamsState } from "hooks"
import { Publisher } from "types"
import { Badge, ButtonPrimary, TableColumn, TableItem, Input, Pagination, Table, TableEmptyState } from "ui/components"
import { renderAuthor, renderBannedStatus } from "ui/renderers"
import { isModeratorCanCreate, isPublisherCanCreate, parseInteger, routes } from "utils"

const getItemRenderer =
  (t: TFunction, storeId: string, pathname: string, publisherIds?: string[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const publisher = item as unknown as Publisher

    switch (column.type) {
      case "author":
        return (
          <div className="flex items-center gap-2">
            <Link to={routes.moderation.publisher(storeId, publisher.author.id)}>{renderAuthor(publisher.author)}</Link>
            {publisherIds?.includes(publisher.author.id) && <Badge>{t("common:you")}</Badge>}
          </div>
        )

      case "verified":
        return publisher.verifiedWebdomainRank ? (
          <div className="flex items-center gap-1.5" title={t("verifiedRankTitle")}>
            <SvgCheckCircleFill className="size-5 shrink-0 fill-light-green" />
            <span>{publisher.verifiedWebdomainRank}</span>
          </div>
        ) : (
          ""
        )

      case "status":
        return renderBannedStatus(t, publisher.bannedTill)

      case "actions":
        return (
          <div className="flex justify-end">
            <Link
              to={routes.moderation.createProposal(storeId)}
              state={{
                parentBreadcrumbs: [{ path: routes.moderation.publishers(storeId), title: t("common:proposals") }],
                previousPath: pathname,
                title: truncate(`Remove author "${publisher.author.title}"`, { length: 64 }),
                type: "store-authors-removal",
                authors: [publisher.author],
                redirectAfterProposalCreation: routes.moderation.publishers(storeId, "proposals"),
                redirectAfterProposalExecution: location.pathname,
              }}
            >
              <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:remove")} />
            </Link>
          </div>
        )
    }
  }

export const PublishersListTab = () => {
  const { t } = useTranslation("publishersPage")

  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher, publisherIds } = useStoreRolesContext()

  const canRemovePublisher =
    (isPublisher && isPublisherCanCreate("store-authors-removal", policies)) ||
    (isModerator && isModeratorCanCreate("store-authors-removal", policies))

  const storeId = useResolveStoreId()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
    query: {
      defaultValue: "",
      validate: v => v !== "",
    },
  })

  const [query, setQuery] = useState(state.query)
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: publishers } = useGetStorePublishers(storeId, state.page, DEFAULT_PAGE_SIZE_20, debouncedQuery)
  const pagesCount =
    publishers?.totalItems && publishers.totalItems > 0 ? Math.ceil(publishers.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("columns:author"), type: "author", className: "w-[55%]" },
      { accessor: "verifiedWebdomainRank", label: t("columns:verifiedDomain"), type: "verified", className: "w-[15%]" },
      { accessor: "bannedTill", label: t("columns:status"), type: "status", className: "w-[20%]" },
      ...(canRemovePublisher
        ? [
            {
              accessor: "actions",
              label: t("columns:actions"),
              type: "actions",
              className: "w-[10%] text-center",
            },
          ]
        : []),
    ],
    [t, canRemovePublisher],
  )

  const itemRenderer = useMemo(
    () => getItemRenderer(t, storeId!, location.pathname, publisherIds),
    [publisherIds, storeId, t],
  )

  const items = useMemo(
    () =>
      publishers?.items.map(x => ({
        id: x.author.id,
        ...x,
      })),
    [publishers],
  )

  const handleInputClear = useCallback(() => setQuery(""), [])

  const handlePageChange = useCallback(
    (page: number) => setState({ query: debouncedQuery, page }),
    [debouncedQuery, setState],
  )

  useEffect(() => {
    if (debouncedQuery !== state.query) {
      setState({ query: debouncedQuery, page: 0 })
    }
  }, [debouncedQuery, query, setState, state])

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Input
          value={query}
          onChange={setQuery}
          placeholder={t("placeholders:enterPublisherNameOrId")}
          className="placeholder:text-gray-500"
          containerClassName="max-w-120"
          elementAfter={
            <>
              {query && (
                <div onClick={handleInputClear} className="cursor-pointer">
                  <SvgX className="stroke-gray-400 hover:stroke-gray-950" />
                </div>
              )}
              <SvgSearchMd className="size-5 shrink-0 stroke-gray-500" />
            </>
          }
        />
      </div>
      <Table
        columns={columns}
        items={items}
        itemRenderer={itemRenderer}
        tableBodyClassName="text-2sm leading-5"
        emptyState={<TableEmptyState message={t("noPublishers")} />}
      />
      <div className="flex w-full justify-end">
        <Pagination pagesCount={pagesCount} onPageChange={handlePageChange} page={state.page} />
      </div>
    </div>
  )
}
