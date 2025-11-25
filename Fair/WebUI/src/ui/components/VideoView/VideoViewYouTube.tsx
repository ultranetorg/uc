import { memo } from "react"
import { useTranslation } from "react-i18next"

import { VideoFrame } from "ui/components/VideoFrame"
import { urlRegexMap } from "utils"

export const VideoViewYouTube = memo(({ url, className }: { url: string; className?: string }) => {
  const { t } = useTranslation("productFields")

  const videoIdMatch = url.match(urlRegexMap.youtube)
  const videoId = videoIdMatch ? videoIdMatch[1] : null

  if (!videoId) {
    return <div> {t("invalidUrl")} </div>
  }

  const embedUrl = `https://www.youtube.com/embed/${videoId}`

  return (
    <div className={className}>
      <VideoFrame src={embedUrl} title="YouTube video player" />
    </div>
  )
})
