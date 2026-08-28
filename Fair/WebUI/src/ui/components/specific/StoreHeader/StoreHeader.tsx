import { KeyboardEvent, useCallback, useEffect, useMemo, useState } from "react"
import { useMatch, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useStoreContext, useSearchQueryContext, useStoreRolesContext, useUserContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchLitePublications } from "entities"
import { useResolveStoreId } from "hooks"
import { SearchDropdown, SearchDropdownItem } from "ui/components"
import { routes } from "utils"

import { GovernanceDropdownButton } from "./GovernanceDropdownButton"
import { ModerationDropdownButton } from "./ModerationDropdownButton"
import { PublisherMembersDropdownButton } from "./PublisherMembersDropdownButton"
import { UserProfileButton } from "./UserProfileButton"

export const StoreHeader = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const isSearchPage = useMatch("/:storeId/s")
  const { store } = useStoreContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { t } = useTranslation("storePage")
  const { user } = useUserContext()

  const { query: storeQuery, setQuery: setStoreQuery } = useSearchQueryContext()

  const [query, setQuery] = useState(storeQuery)

  // Keeps the input in sync when the active query comes from outside a header interaction,
  // e.g. landing on the search page via a link that already carries a query param.
  useEffect(() => {
    setQuery(storeQuery)
  }, [storeQuery])

  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: publication, isFetching } = useSearchLitePublications(storeId, debouncedQuery, !!isSearchPage)
  const items = useMemo(
    () => (!isSearchPage ? publication?.map(x => ({ value: x.id, label: x.title })) : undefined),
    [isSearchPage, publication],
  )

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(routes.publication(item.value))
      }
    },
    [navigate, storeId],
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
        setStoreQuery(query)
        navigate(routes.search(storeId!))
      }
    },
    [query, navigate, storeId, setStoreQuery],
  )

  const handleSearchClick = useCallback(() => {
    if (query) {
      setStoreQuery(query)
    }
  }, [query, setStoreQuery])

  if (!store || !storeId) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 pb-8">
      <div className="flex w-135 items-center justify-between gap-4">
        <SearchDropdown
          key={storeQuery}
          size="medium"
          className="grow"
          isLoading={isFetching}
          inputValue={storeQuery}
          items={items}
          onChange={handleChange}
          onClearInputClick={handleClearInputClick}
          onInputChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onSearchClick={handleSearchClick}
        />
      </div>
      <div className="flex items-center gap-8">
        <GovernanceDropdownButton className="w-28" />
        {isModerator && <ModerationDropdownButton className="w-28" />}
        {isPublisher && <PublisherMembersDropdownButton className="w-25" storeId={storeId} t={t} user={user!} />}
        <UserProfileButton storeId={storeId} t={t} />
      </div>
    </div>
  )
}
