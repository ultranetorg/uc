import { useMemo, useState } from "react"

import type { Swiper as SwiperClass } from "swiper/types"

import { Swiper, SwiperSlide } from "swiper/react"
import { EffectFade, Navigation, Pagination } from "swiper/modules"

import "swiper/css"
import "swiper/css/effect-fade"
import "swiper/css/navigation"
import "swiper/css/pagination"

import "./styles.css"
import { SvgChevronLeftMd, SvgChevronRightMd } from "assets"

import { VideoType } from "types"
import { VideoViewPlain, VideoViewVk, VideoViewYouTube } from "ui/components"
import { getVideoType, urlRegexMap } from "utils"

export type SliderMediaItem = {
  id?: string | number
  src: string
  poster?: string
  alt?: string
}

export type SliderProps = {
  items?: SliderMediaItem[]
}

function inferType(src: string): "image" | VideoType {
  const clean = src.split("#")[0]
  const type = getVideoType(clean)
  return type ?? "image"
}

function inferPoster(src: string): string | undefined {
  const type = inferType(src)

  if (type === "youtube") {
    const match = src.match(urlRegexMap.youtube)
    const id = match ? match[1] : null
    return id ? `https://img.youtube.com/vi/${id}/hqdefault.jpg` : undefined
  }

  return undefined
}

function getSlidePoster(item: SliderMediaItem): string | undefined {
  const type = inferType(item.src)

  if (type === "image") {
    return item.src
  }

  if (type === "youtube") {
    return inferPoster(item.src) ?? item.poster
  }

  return item.poster ?? inferPoster(item.src)
}

const VideoPlayOverlay = () => (
  <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
    <div className="flex h-11 w-11 items-center justify-center rounded-full bg-black/45">
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
        <path d="M10 8L17 12L10 16V8Z" fill="white" />
      </svg>
    </div>
  </div>
)

const SlideContent = ({ item, isActive }: { item: SliderMediaItem; isActive: boolean }) => {
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
        <VideoViewPlain url={item.src} className="size-full object-cover" controls={true} muted={true} posterUrl={item.poster} />
      )
    case "youtube":
      return <VideoViewYouTube url={item.src} className="h-full w-full" />
    case "vkVideo":
      return <VideoViewVk url={item.src} className="h-full w-full" />
    default:
      return null
  }
}

export const Slider = ({ items = [] }: SliderProps) => {
  if (!items.length) {
    return (
      <div className="h-[416px] max-w-[750px] overflow-hidden rounded-xl bg-gradient-to-r from-[#0b1e3a] to-[#29593f] text-white" />
    )
  }

  const [swiper, setSwiper] = useState<SwiperClass | null>(null)
  const [activeIndex, setActiveIndex] = useState(0)

  const thumbs = useMemo(() => {
    return items.map(item => {
      const type = inferType(item.src)
      const isVideo = type !== "image"
      const thumbSrc = type === "youtube" ? inferPoster(item.src) ?? item.poster : item.poster ?? (isVideo ? inferPoster(item.src) : item.src)
      return { src: thumbSrc, isVideo }
    })
  }, [items])

  return (
    <div className="flex max-w-[750px] flex-col gap-3">
      <div className="relative h-[416px] overflow-hidden rounded-xl bg-gradient-to-r from-[#0b1e3a] to-[#29593f] text-white">
        <Swiper
          className="h-full w-full"
          effect="fade"
          modules={[EffectFade, Navigation, Pagination]}
          navigation={{
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
          }}
          pagination={{
            clickable: true,
            renderBullet: (_: number, className: string) => `<div class="${className}"></div>`,
          }}
          onSwiper={setSwiper}
          onSlideChange={(s) => setActiveIndex(s.activeIndex)}
        >
          {items.map((item, index) => (
            <SwiperSlide key={String(item.id ?? item.src ?? index)}>
              <SlideContent item={item} isActive={index === activeIndex} />
            </SwiperSlide>
          ))}
        </Swiper>
        <div className="swiper-button-prev absolute left-2 top-1/2 z-10 flex h-11 w-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-white/10">
          <SvgChevronLeftMd className="stroke-white" />
        </div>
        <div className="swiper-button-next absolute right-2 top-1/2 z-10 flex h-11 w-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-white/10">
          <SvgChevronRightMd className="stroke-white" />
        </div>
      </div>

      {items.length > 1 ? (
        <div className="flex gap-2 overflow-auto pb-1">
          {thumbs.map((t, idx) => (
            <button
              key={`${t.src}-${idx}`}
              type="button"
              onClick={() => swiper?.slideTo?.(idx)}
              className={
                idx === activeIndex
                  ? "h-14 w-22 shrink-0 overflow-hidden rounded border-2 border-primary outline-none focus:outline-none focus-visible:outline-none"
                  : "h-14 w-22 shrink-0 overflow-hidden rounded border-2 border-gray-300 outline-none focus:outline-none focus-visible:outline-none"
              }
            >
              <div className="relative h-full w-full">
                {t.src ? <img src={t.src} alt="" className="h-full w-full object-cover" /> : <div className="h-full w-full bg-black/30" />}
                {t.isVideo ? (
                  <VideoPlayOverlay />
                ) : null}
              </div>
            </button>
          ))}
        </div>
      ) : null}
    </div>
  )
}
