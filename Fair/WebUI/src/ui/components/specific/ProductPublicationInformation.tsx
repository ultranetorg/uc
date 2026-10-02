import { memo } from "react"
import { useTranslation } from "react-i18next"

import { ProductDetails, PublicationChanged, PublicationDetails } from "types"
import { formatDate } from "utils"

const LABEL_CLASSNAME = "text-sm font-medium leading-4"
const VALUE_CLASSNAME = "truncate text-sm leading-4"

export interface ProductPublicationInformationProps {
  product?: ProductDetails
  publication?: PublicationDetails
  publicationChanged?: PublicationChanged
}

export const ProductPublicationInformation = memo(
  ({ product, publication, publicationChanged }: ProductPublicationInformationProps) => {
    const { t } = useTranslation("productPublicationInformation")
    const { type, updated } = (product ?? publication ?? publicationChanged)!

    return (
      <div className="flex flex-col gap-4">
        <div className="flex gap-2">
          <span className={LABEL_CLASSNAME}>{t("productType")}:</span>
          <span className={VALUE_CLASSNAME}>{t("categoryTypes:" + type)}</span>
        </div>
        <div className="flex gap-2">
          <span className={LABEL_CLASSNAME}>{t("updationDate")}:</span>
          <span className={VALUE_CLASSNAME}>{formatDate(updated)}</span>
        </div>
      </div>
    )
  },
)
