import { useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"
import { isNumber } from "lodash"

import { useModerationContext } from "app"
import { SvgSearchMd, SvgX } from "assets"
import { DEFAULT_PAGE_SIZE_20, SEARCH_DELAY } from "config"
import { useGetSitePublishers } from "entities"
import { useUrlParamsState } from "hooks"
import { Input, Pagination, Table, TableEmptyState } from "ui/components"
import { parseInteger } from "utils"

import { getPublishersTabItemRenderer } from "./publishersTabItemRenderer"

export const PublishersTab = () => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation("publishersPage")
  const voterId = getOperationVoterId("site-authors-removal")

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

  const { data: publishers } = useGetSitePublishers(siteId, state.page, DEFAULT_PAGE_SIZE_20, debouncedQuery)
  const pagesCount =
    publishers?.totalItems && publishers.totalItems > 0 ? Math.ceil(publishers.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const columns = useMemo(
    () => [
      { accessor: "author", label: t("common:author"), type: "author", className: "w-[45%]" },
      { accessor: "bannedTill", label: t("bannedTill"), type: "banned", className: "w-[40%]" },
      ...(voterId
        ? [
            {
              accessor: "actions",
              label: t("common:actions"),
              type: "actions",
              className: "w-[10%] text-center first-letter:uppercase",
            },
          ]
        : []),
    ],
    [t, voterId],
  )

  const itemRenderer = useMemo(() => getPublishersTabItemRenderer(t, siteId!, location.pathname), [siteId, t])

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
          iconAfter={
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
