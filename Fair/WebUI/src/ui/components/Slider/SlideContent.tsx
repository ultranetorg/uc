import { memo } from "react"
import { VideoViewPlain, VideoViewVk, VideoViewYouTube } from "ui/components/VideoView"

import { SliderItem } from "./types"
import { getSlidePoster, inferType } from "./utils"
import { VideoPlayOverlay } from "./VideoPlayOverlay"

export const SlideContent = memo(({ item, isActive }: { item: SliderItem; isActive: boolean }) => {
  const type = inferType(item.src)

  if (!isActive && type !== "image") {
    const poster = getSlidePoster(item)

    return poster ? (
      <div className="relative size-full">
        <img src={poster} alt={item.alt ?? ""} className="size-full object-cover" />
        <VideoPlayOverlay />
      </div>
    ) : (
      <div className="size-full bg-black/30" />
    )
  }

  switch (type) {
    case "image":
      return <img src={item.src} alt={item.alt ?? ""} className="size-full object-cover" />
    case "video":
      return (
        <VideoViewPlain
          url={item.src}
          className="size-full object-cover"
          controls={true}
          muted={true}
          posterUrl={item.poster}
        />
      )
    case "youtube":
      return <VideoViewYouTube url={item.src} className="size-full" />
    case "vkVideo":
      return <VideoViewVk url={item.src} className="size-full" />
    default:
      return null
  }
})
