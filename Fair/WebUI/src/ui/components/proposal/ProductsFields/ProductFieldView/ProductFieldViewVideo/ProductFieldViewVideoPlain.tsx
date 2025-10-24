import { memo } from "react"
import { useTranslation } from "react-i18next"

export const ProductFieldViewVideoPlain = memo(({ url }: { url: string }) => {
  const { t } = useTranslation("productToken")

  return (
    <video controls className="w-full h-full bg-black">
      <source src={url} />
      {t("unsupportedTag")}
    </video>
  )
})
