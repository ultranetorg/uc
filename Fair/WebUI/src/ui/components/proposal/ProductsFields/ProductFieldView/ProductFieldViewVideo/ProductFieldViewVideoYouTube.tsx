import { memo } from "react"
import { useTranslation } from "react-i18next"
import { VideoFrame } from "ui/components"

export const ProductFieldViewVideoYouTube = memo(({ url, regex }: { url: string; regex: RegExp }) => {
  const { t } = useTranslation("productToken")

  const videoIdMatch = url.match(regex)
  const videoId = videoIdMatch ? videoIdMatch[1] : null

  if (!videoId) {
    return <div> {t("invalidUrl")} </div>
  }

  const embedUrl = `https://www.youtube.com/embed/${videoId}`

  return <VideoFrame src={embedUrl} title="YouTube video player" />
})
