import { memo } from "react"
import { useTranslation } from "react-i18next"

import { ProductDetails, PublicationDetails } from "types"
import { formatDate } from "utils"

const LABEL_CLASSNAME = "text-sm font-medium leading-4"
const VALUE_CLASSNAME = "truncate text-sm leading-4"

export interface ProductPublicationInformationProps {
  product: ProductDetails | PublicationDetails
}

export const ProductPublicationInformation = memo(({ product }: ProductPublicationInformationProps) => {
  const { t } = useTranslation("productPublicationInformation")

  return (
    <div className="flex flex-col gap-4">
      <div className="flex gap-2">
        <span className={LABEL_CLASSNAME}>{t("productType")}:</span>
        <span className={VALUE_CLASSNAME}>{t("categoryTypes:" + product.type)}</span>
      </div>
      <div className="flex gap-2">
        <span className={LABEL_CLASSNAME}>{t("updationDate")}:</span>
        <span className={VALUE_CLASSNAME}>{formatDate(product.updated)}</span>
      </div>
    </div>
  )
})
