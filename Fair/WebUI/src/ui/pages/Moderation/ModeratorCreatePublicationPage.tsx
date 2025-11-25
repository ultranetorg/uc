import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useGetUnpublishedProduct } from "entities"
import { SearchDropdown, SearchDropdownItem } from "ui/components"
import { ModeratorPublicationHeader } from "ui/components/specific"

export const ModeratorCreatePublicationPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("createPublication")

  const [query, setQuery] = useState("")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)
  const { data: unpublishedProduct, isFetching } = useGetUnpublishedProduct(debouncedQuery)

  const handleChange = useCallback((item?: SearchDropdownItem) => {
    console.log(item)
  }, [])

  const handleClearInputClick = useCallback(() => {
    setQuery("")
  }, [setQuery])

  const handleInputChange = useCallback(
    (value: string) => {
      setQuery(value)
    },
    [setQuery],
  )

  return (
    <div className="flex flex-col gap-6">
      <ModeratorPublicationHeader
        className="gap-2"
        siteId={siteId!}
        parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/n/` }}
        title={t("searchProduct")}
        showLogo={false}
        homeLabel={t("common:home")}
      />
      <SearchDropdown
        size="medium"
        className="max-w-120"
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
      />
      {JSON.stringify(unpublishedProduct)}
    </div>
  )
}
