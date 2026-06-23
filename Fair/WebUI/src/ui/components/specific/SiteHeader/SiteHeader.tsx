import { KeyboardEvent, useCallback, useMemo, useState } from "react"
import { useMatch, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext, useSearchQueryContext, useSiteRolesContext, useUserContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchLitePublications } from "entities"
import { useResolveSiteId } from "hooks"
import { SearchDropdown, SearchDropdownItem } from "ui/components"
import { routes } from "utils"

import { CategoriesDropdownButton } from "./CategoriesDropdownButton"
import { GovernanceDropdownButton } from "./GovernanceDropdownButton"
import { LogoDropdownButton } from "./LogoDropdownButton"
import { ModerationDropdownButton } from "./ModerationDropdownButton"
import { UserProfileButton } from "./UserProfileButton"
import { toSimpleMenuItems } from "./utils"
import { PublisherMembersDropdownButton } from "./PublisherMembersDropdownButton"

export const SiteHeader = () => {
  const siteId = useResolveSiteId()
  const navigate = useNavigate()
  const isSearchPage = useMatch("/:siteId/s")
  const { site, rootCategories } = useSiteContext()
  const { isModerator, isPublisher } = useSiteRolesContext()
  const { t } = useTranslation("site")
  const { user } = useUserContext()

  const { setQuery: setSiteQuery } = useSearchQueryContext()

  const [query, setQuery] = useState("")
  const categoriesItems = useMemo(
    () => (rootCategories && siteId ? toSimpleMenuItems(rootCategories, siteId) : undefined),
    [rootCategories, siteId],
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
        navigate(routes.publication(siteId!, item.value))
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
        navigate(routes.search(siteId!))
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
      <LogoDropdownButton
        t={t}
        siteId={siteId!}
        title={site.title}
        imageFileId={site.imageFileId}
        publishersCount={site.authorsIds.length}
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
        {isPublisher && <PublisherMembersDropdownButton className="w-25" t={t} siteId={siteId!} user={user!} />}
        <UserProfileButton t={t} />
      </div>
    </div>
  )
}
