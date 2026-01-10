import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SvgSearchMd, SvgX } from "assets"
import { SEARCH_DELAY } from "config"
import { useGetUnpublishedSiteProduct } from "entities"
import { Input, MessageBox } from "ui/components"
import { ModeratorPublicationHeader, ProductFieldsViewer } from "ui/components/specific"

export const ModeratorCreatePublicationPage = () => {
  const { siteId } = useParams()
  const { t } = useTranslation("createPublication")

  const [query, setQuery] = useState("")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)
  const { data: product } = useGetUnpublishedSiteProduct(siteId, debouncedQuery)

  const handleInputClear = useCallback(() => {
    setQuery("")
  }, [setQuery])

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
        onApprove={product ? handleApprove : undefined}
        onPreview={product ? handlePreview : undefined}
        onReject={product ? handleReject : undefined}
        homeLabel={t("common:home")}
      />
      <div className="max-w-120">
        <Input
          value={query}
          onChange={setQuery}
          placeholder={t("placeholders:enterProductId")}
          className="max-w-120 placeholder:text-gray-500"
          iconAfter={
            <>
              {query && (
                <div onClick={handleInputClear} className="cursor-pointer">
                  <SvgX className="stroke-gray-400 hover:stroke-gray-950" />
                </div>
              )}
              <SvgSearchMd className="size-5 stroke-gray-500" />
            </>
          }
        />
      </div>

      {debouncedQuery && !product && <MessageBox className="p-6" message={t("productNotFound")} />}
      {product && <ProductFieldsViewer productFields={product.versions} />}
    </div>
  )
}
