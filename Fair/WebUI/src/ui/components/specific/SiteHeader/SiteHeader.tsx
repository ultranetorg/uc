import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { Link, useMatch, useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSearchQueryContext, useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchLitePublications } from "entities"
import { SearchDropdown, SearchDropdownItem } from "ui/components"

import { CategoriesDropdownButton } from "./CategoriesDropdownButton"
import { LinkCounter } from "./LinkCounter"
import { LogoDropdownButton } from "./LogoDropdownButton"
import { toSimpleMenuItems } from "./utils"

export const SiteHeader = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()
  const isSearchPage = useMatch("/:siteId/s")

  const { t } = useTranslation("site")

  const { site } = useSiteContext()
  const { setQuery: setSiteQuery } = useSearchQueryContext()
  const [query, setQuery] = useState("")
  const categoriesItems = useMemo(
    () => (site?.categories && siteId ? toSimpleMenuItems(site?.categories, siteId) : undefined),
    [site, siteId],
  )

  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: publication, isFetching } = useSearchLitePublications(siteId, debouncedQuery, !!isSearchPage)
  const items = useMemo(
    () => (!isSearchPage ? publication?.map(x => ({ value: x.id, label: x.title })) : undefined),
    [isSearchPage, publication],
  )

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(`/${siteId}/p/${item.value}`)
      }
    },
    [navigate, siteId],
  )

  const handleClearInputClick = useCallback(() => {
    setQuery("")
  }, [setQuery])

  const handleInputChange = useCallback(
    (value: string) => {
      setQuery(value)
    },
    [setQuery],
  )

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Enter" && query) {
        setSiteQuery(query)
        navigate(`/${siteId}/s`)
      }
    },
    [query, navigate, siteId, setSiteQuery],
  )

  const handleSearchClick = useCallback(() => {
    if (query) {
      setSiteQuery(query)
    }
  }, [query, setSiteQuery])

  if (!site) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 pb-8">
      <Link to={`/${siteId}`}>
        <LogoDropdownButton title={site.title} avatar={site.avatar} />
      </Link>
      {categoriesItems && categoriesItems.length > 0 && (
        <CategoriesDropdownButton label={t("categories")} className="w-[105px]" items={categoriesItems} />
      )}
      <SearchDropdown
        size="medium"
        className="grow"
        isLoading={isFetching}
        items={items}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onSearchClick={handleSearchClick}
      />
      <LinkCounter to={`/${siteId}/g`} className="w-[115px]">
        {t("governance")}
      </LinkCounter>
      <LinkCounter to={`/${siteId}/m`} className="w-[110px]">
        {t("moderation")}
      </LinkCounter>
      {site.description && (
        <LinkCounter to={`/${siteId}/i`} className="w-[45px]">
          {t("about")}
        </LinkCounter>
      )}
    </div>
  )
}
