import { memo } from "react"
import { useTranslation } from "react-i18next"

export const VideoViewPlain = memo(
  ({
    url,
    className,
    posterUrl,
    controls,
    muted,
  }: {
    url: string
    className?: string
    posterUrl?: string
    muted?: boolean
    controls?: boolean
  }) => {
    const { t } = useTranslation("common")

    return (
      <video controls={controls} preload="metadata" playsInline muted={muted} className={className} poster={posterUrl}>
        <source src={url} />
        {t("unsupportedTag")}
      </video>
    )
  },
)
