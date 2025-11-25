import React, { useEffect, useMemo, useRef, useState } from "react"
import { SvgChevronLeftMd, SvgChevronRightMd } from "assets"
import { getVideoType } from "utils"
import { VideoType } from "types"
import { VideoViewYouTube, VideoViewVk, VideoViewPlain } from "ui/components"
import { IPublicationDescription } from "../types"
import { PublicationViewDescription } from "./PublicationViewDescription"

export type MediaItem = {
  id?: string | number
  src: string // either an absolute url or a path relative to API files
  type?: "image" | "video"
  alt?: string
  title?: IPublicationDescription[]
  description?: IPublicationDescription[]
  poster?: string // optional poster for videos
}

export type PublicationViewArtsProps = {
  items?: MediaItem[]
}

function inferTypeFromSrc(src: string): "image" | VideoType {
  const s = src.split("?")[0].split("#")[0].toLowerCase()
  if (s.match(/\.(mp4|webm|ogg)$/)) return "video"

  return getVideoType(s) ?? "image"
}

const VideoContent = ({
  src,
  type,
  className,
  poster,
  muted,
  controls,
}: {
  src: string
  type: "image" | VideoType
  className: string
  poster?: string
  muted?: boolean
  controls?: boolean
}) => {
  switch (type) {
    case "image":
      return <img key={src} src={src} alt="" className={className} />
    case "video":
      return <VideoViewPlain url={src} className={className} controls={controls} posterUrl={poster} muted={muted} />
    case "youtube":
      return <VideoViewYouTube url={src} className={className} />
    case "vkVideo":
      return <VideoViewVk url={src} className={className} />
  }
}

export const PublicationViewArts: React.FC<PublicationViewArtsProps> = ({ items = [] }) => {
  const normalized = useMemo(() => {
    return items.map(it => {
      const type = it.type ?? inferTypeFromSrc(it.src)
      return { ...it, src: it.src, type }
    })
  }, [items])

  const [activeIndex, setActiveIndex] = useState<number>(0)
  const thumbListRef = useRef<HTMLDivElement | null>(null)
  const activeThumbRef = useRef<HTMLButtonElement | null>(null)

  // ensure activeIndex stays in bounds when items change
  useEffect(() => {
    if (activeIndex >= normalized.length) setActiveIndex(Math.max(0, normalized.length - 1))
  }, [normalized, activeIndex])

  // scroll active thumbnail into view
  useEffect(() => {
    const btn = activeThumbRef.current
    const container = thumbListRef.current
    if (!btn || !container) return
    const btnRect = btn.getBoundingClientRect()
    const contRect = container.getBoundingClientRect()
    // if not fully visible, scroll the container
    if (btnRect.top < contRect.top || btnRect.bottom > contRect.bottom) {
      btn.scrollIntoView({ block: "nearest", behavior: "smooth" })
    }
  }, [activeIndex])

  // keyboard navigation: left/right arrows change active media
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "ArrowRight") {
        setActiveIndex(i => Math.min(normalized.length - 1, i + 1))
      } else if (e.key === "ArrowLeft") {
        setActiveIndex(i => Math.max(0, i - 1))
      }
    }
    window.addEventListener("keydown", onKey)
    return () => window.removeEventListener("keydown", onKey)
  }, [normalized.length])

  if (normalized.length === 0) {
    return null
  }

  const active = normalized[activeIndex]

  return (
    <div className="flex w-full max-w-md flex-col gap-2">
      <div
        ref={thumbListRef}
        className="flex items-start gap-2 overflow-x-auto overflow-y-hidden"
        aria-label="Media thumbnails"
      >
        {normalized.map((it, idx) => {
          const isActive = idx === activeIndex
          const key = String(it.id ?? it.src)
          return (
            <button
              key={key}
              ref={isActive ? activeThumbRef : undefined}
              onClick={() => setActiveIndex(idx)}
              className={`relative ${isActive ? "border-4 border-dashed border-gray-300 bg-gray-600 bg-clip-content p-2" : ""}`}
              aria-pressed={isActive}
              aria-label={`Show media ${idx + 1}`}
              aria-current={isActive ? "true" : undefined}
            >
              <div className={`${isActive ? "size-16" : "size-20"}`}>
                <VideoContent src={it.poster ?? it.src} className="size-full object-cover" type="image" />
              </div>
            </button>
          )
        })}
      </div>

      {/* right/main viewer */}
      <div className="flex flex-1 flex-col">
        {active.title && <PublicationViewDescription data={active.title} label="descriptionPoster" />}

        <div
          className="relative flex w-full items-center justify-center overflow-hidden rounded bg-black"
          style={{ minHeight: 320 }}
        >
          {/* main content */}
          <VideoContent
            src={active.src}
            className="size-full object-contain"
            type={active.type}
            poster={active.poster}
            controls={true}
          />

          {/* left/right controls (for large screens) */}
          {normalized.length > 1 && (
            <>
              <button
                onClick={() => setActiveIndex(i => Math.max(0, i - 1))}
                className="absolute left-2 top-1/2 hidden size-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-white/10"
                aria-label="Previous media"
              >
                <SvgChevronLeftMd className="stroke-white" />
              </button>

              <button
                onClick={() => setActiveIndex(i => Math.min(normalized.length - 1, i + 1))}
                className="absolute right-2 top-1/2 hidden size-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-white/10"
                aria-label="Next media"
              >
                <SvgChevronRightMd className="stroke-white" />
              </button>
            </>
          )}
        </div>

        {active.description && <PublicationViewDescription data={active.description} label="descriptionVideo" />}
      </div>
    </div>
  )
}

export default PublicationViewArts
