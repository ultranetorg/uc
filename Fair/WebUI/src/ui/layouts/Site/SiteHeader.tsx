import { KeyboardEvent, useCallback, useMemo } from "react"
import { Link, useNavigate, useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { useSearchQueryContext, useSiteContext } from "app"
import { ChatXSvg, PersonKingSvg } from "assets"
import { SEARCH_DELAY } from "constants"
import { useSearchLitePublications } from "entities"
import { Button, Logo, SearchDropdown, SearchDropdownItem } from "ui/components"

import { CategoriesButton } from "./components"

export const SiteHeader = () => {
  const { siteId } = useParams()
  const navigate = useNavigate()

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
    [siteId, navigate],
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
      <Link to={`/${siteId}`}>
        <Logo title={site.title} />
      </Link>
      <CategoriesButton siteId={siteId!} />
      <Link to={`/${siteId}/m-d`}>
        <Button className="gap-2" image={<ChatXSvg className="fill-zinc-700 stroke-zinc-700" />} label="Disputes" />
      </Link>
      <Link to={`/${siteId}/m`}>
        <Button className="gap-2" image={<PersonKingSvg className="stroke-zinc-700" />} label="Moderation" />
      </Link>
      <SearchDropdown
        className="flex-grow"
        isLoading={isFetching}
        items={items}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onSearchClick={handleSearchClick}
      />
    </div>
  )
}
