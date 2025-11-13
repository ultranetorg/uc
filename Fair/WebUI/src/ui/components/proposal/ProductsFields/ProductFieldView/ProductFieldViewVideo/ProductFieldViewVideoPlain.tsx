import { memo } from "react"
import { useTranslation } from "react-i18next"

export const ProductFieldViewVideoPlain = memo(({ url }: { url: string }) => {
  const { t } = useTranslation("productFields")

  return (
    <video controls className="h-full w-full bg-black">
      <source src={url} />
      {t("unsupportedTag")}
    </video>
  )
})
