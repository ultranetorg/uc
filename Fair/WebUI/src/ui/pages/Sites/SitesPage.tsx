import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"
import { isNumber } from "lodash"

import { SEARCH_DELAY } from "constants"
import { useGetDefaultSites, useSearchLiteSites, useSearchSites } from "entities"
import { useUrlParamsState } from "hooks"
import { SearchDropdown, SearchDropdownItem, SitesList } from "ui/components"
import { parseInteger } from "utils"

import { PageHeader } from "./PageHeader"

export const SitesPage = () => {
  const { t } = useTranslation("sites")
  const navigate = useNavigate()

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
  const [liteQuery, setLiteQuery] = useState("")
  const [debouncedLiteQuery] = useDebounceValue(liteQuery, SEARCH_DELAY)

  const { data: defaultSites, isFetching: isDefaultSitesFetching } = useGetDefaultSites(!query)

  const { data: liteSites, isFetching: isLiteFetching } = useSearchLiteSites(debouncedLiteQuery)
  const liteItems = useMemo(() => liteSites?.map(x => ({ value: x.id, label: x.title })), [liteSites])

  const { isFetching, data: sites, error } = useSearchSites(query, state.page)

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(`/${item.value}`)
      }
    },
    [navigate],
  )

  const handleClearInputClick = useCallback(() => {
    setLiteQuery("")
    setQuery("")
    setState()
  }, [setState])

  const handleInputChange = useCallback((value: string) => setLiteQuery(value), [])

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Enter") {
        setQuery(liteQuery)
        setState({ query: liteQuery, page: 0 })
      }
    },
    [liteQuery, setState],
  )

  const handleSearchClick = useCallback(() => {
    setQuery(liteQuery)
    setState({ query: liteQuery, page: 0 })
  }, [liteQuery, setState])

  return (
    <div className="flex flex-col items-center gap-6 py-16">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="w-full max-w-[820px]">
        <div className="flex w-full">
          <SearchDropdown
            className="w-full"
            isLoading={isLiteFetching}
            inputValue={state.query}
            items={liteItems}
            onChange={handleChange}
            onClearInputClick={handleClearInputClick}
            onInputChange={handleInputChange}
            onKeyDown={handleKeyDown}
            onSearchClick={handleSearchClick}
          />
        </div>
      </div>
      <SitesList
        isFetching={isFetching ?? isDefaultSitesFetching}
        items={sites?.items ?? defaultSites}
        itemsCount={sites?.items.length ?? defaultSites?.length ?? 0}
        error={error}
      />
    </div>
  )
}
