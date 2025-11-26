import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useHeadUnpublishedProduct } from "entities"
import { MessageBox, SearchDropdown, SearchDropdownItem } from "ui/components"
import { ModeratorPublicationHeader } from "ui/components/specific"
import { ProductFields } from "ui/components/proposal"

export const ModeratorCreatePublicationPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("createPublication")

  const [productId, setProductId] = useState<string | undefined>()
  const [query, setQuery] = useState("")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)
  const { data: productExists } = useHeadUnpublishedProduct(debouncedQuery)

  const items = useMemo(
    () => (productExists ? [{ label: debouncedQuery, value: debouncedQuery }] : undefined),
    [debouncedQuery, productExists],
  )

  const handleChange = useCallback((item?: SearchDropdownItem) => {
    setProductId(item!.value)
  }, [])

  const handleClearInputClick = useCallback(() => {
    setProductId(undefined)
    setQuery("")
  }, [setQuery])

  const handleInputChange = useCallback(
    (value: string) => {
      setProductId(undefined)
      setQuery(value)
    },
    [setQuery],
  )

  const handleApprove = useCallback(() => alert("approve"), [])
  const handleReject = useCallback(() => alert("reject"), [])
  const handlePreview = useCallback(() => alert("preview"), [])

  return (
    <div className="flex flex-col gap-6">
      <ModeratorPublicationHeader
        className="gap-2"
        siteId={siteId!}
        parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/n/` }}
        title={t("searchProduct")}
        showLogo={false}
        onApprove={productId ? handleApprove : undefined}
        onPreview={productId ? handlePreview : undefined}
        onReject={productId ? handleReject : undefined}
        homeLabel={t("common:home")}
      />
      <SearchDropdown
        clearInputAfterChange={false}
        size="medium"
        placeholder={t("placeholders:enterProductId")}
        className="max-w-120"
        items={items}
        onChange={handleChange}
        onClearInputClick={handleClearInputClick}
        onInputChange={handleInputChange}
      />
      {debouncedQuery && !productExists && <MessageBox className="p-6" message={t("productNotFound")} />}
      {productId && <ProductFields productIds={[productId!]} />}
    </div>
  )
}
