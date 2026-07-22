import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useMatch, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useStoreContext, useSearchQueryContext, useStoreRolesContext, useUserContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchLitePublications } from "entities"
import { useResolveStoreId } from "hooks"
import { SearchDropdown, SearchDropdownItem } from "ui/components"
import { routes } from "utils"

import { CategoriesDropdownButton } from "./CategoriesDropdownButton"
import { GovernanceDropdownButton } from "./GovernanceDropdownButton"
import { LogoDropdownButton } from "./LogoDropdownButton"
import { ModerationDropdownButton } from "./ModerationDropdownButton"
import { UserProfileButton } from "./UserProfileButton"
import { toSimpleMenuItems } from "./utils"
import { PublisherMembersDropdownButton } from "./PublisherMembersDropdownButton"

export const StoreHeader = () => {
  const storeId = useResolveStoreId()
  const navigate = useNavigate()
  const isSearchPage = useMatch("/:storeId/s")
  const { store, rootCategories } = useStoreContext()
  const { isModerator, isPublisher } = useStoreRolesContext()
  const { t } = useTranslation("site")
  const { user } = useUserContext()

  const { setQuery: setSiteQuery } = useSearchQueryContext()

  const [query, setQuery] = useState("")
  const categoriesItems = useMemo(
    () => (rootCategories && storeId ? toSimpleMenuItems(rootCategories, storeId) : undefined),
    [rootCategories, storeId],
  )

  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: publication, isFetching } = useSearchLitePublications(storeId, debouncedQuery, !!isSearchPage)
  const items = useMemo(
    () => (!isSearchPage ? publication?.map(x => ({ value: x.id, label: x.title })) : undefined),
    [isSearchPage, publication],
  )

  const handleChange = useCallback(
    (item?: SearchDropdownItem) => {
      if (item) {
        navigate(routes.publication(storeId!, item.value))
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
        setSiteQuery(query)
        navigate(routes.search(storeId!))
      }
    },
    [query, navigate, storeId, setSiteQuery],
  )

  const handleSearchClick = useCallback(() => {
    if (query) {
      setSiteQuery(query)
    }
  }, [query, setSiteQuery])

  if (!store || !storeId) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 pb-8">
      <LogoDropdownButton
        t={t}
        storeId={storeId}
        title={store.title}
        imageFileId={store.imageFileId}
        publishersCount={store.authorsIds.length}
      />
      <div className="flex w-135 items-center justify-between gap-4">
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
      </div>
      <div className="flex items-center gap-8">
        <GovernanceDropdownButton className="w-28" />
        {isModerator && <ModerationDropdownButton className="w-28" />}
        {isPublisher && <PublisherMembersDropdownButton className="w-25" siteId={storeId} t={t} user={user!} />}
        <UserProfileButton storeId={storeId} t={t} />
      </div>
    </div>
  )
}
