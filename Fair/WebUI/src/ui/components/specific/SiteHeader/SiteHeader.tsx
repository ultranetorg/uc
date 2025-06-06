import { KeyboardEvent, useCallback, useMemo } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { useSearchQueryContext, useSiteContext } from "app"
import { SEARCH_DELAY } from "constants"
import { useSearchLitePublications } from "entities"
import { SearchDropdown, SearchDropdownItem } from "ui/components"

import { LinkCounter } from "./LinkCounter"
import { LogoDropdownButton } from "./LogoDropdownButton"
import { useTranslation } from "react-i18next"

export const SiteHeader = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()
  const { t } = useTranslation("siteHeader")

  const { site } = useSiteContext()
  const { query, setQuery, triggerSearchEvent } = useSearchQueryContext()

  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: publication, isFetching } = useSearchLitePublications(siteId, debouncedQuery)
  const items = useMemo(() => publication?.map(x => ({ value: x.id, label: x.title })), [publication])

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
      if (e.key === "Enter" && !!query) {
        navigate(`/${siteId}/s`)

        triggerSearchEvent()
      }
    },
    [query, siteId, triggerSearchEvent, navigate],
  )

  const handleSearchClick = useCallback(() => {
    if (query) {
      triggerSearchEvent()
    }
  }, [query, triggerSearchEvent])

  if (!site) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 py-8">
      <LogoDropdownButton title={site.title} />
      <SearchDropdown
        size="medium"
        className="flex-grow"
        isLoading={isFetching}
        items={items}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onSearchClick={handleSearchClick}
      />
      <LinkCounter count={2} to={`/${siteId}/m-d`}>
        {t("governance")}
      </LinkCounter>
      <LinkCounter count={2} to={`/${siteId}/m`}>
        {t("moderation")}
      </LinkCounter>
      <LinkCounter to={`/${siteId}/b`}>{t("about")}</LinkCounter>
    </div>
  )
}
