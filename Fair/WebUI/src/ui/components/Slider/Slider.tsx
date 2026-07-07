import { memo, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import type { Swiper as SwiperClass } from "swiper/types"
import { Swiper, SwiperSlide } from "swiper/react"
import { EffectFade, Navigation, Pagination } from "swiper/modules"

import "swiper/css"
import "swiper/css/effect-fade"
import "swiper/css/navigation"
import "swiper/css/pagination"

import "./styles.css"

import { SvgChevronLeftMd, SvgChevronRightMd } from "assets"

import { SlideContent } from "./SlideContent"
import { SliderProps } from "./types"
import { inferPoster, inferType } from "./utils"
import { VideoPlayOverlay } from "./VideoPlayOverlay"

export const Slider = memo(({ items = [] }: SliderProps) => {
  const [swiper, setSwiper] = useState<SwiperClass | null>(null)
  const [activeIndex, setActiveIndex] = useState(0)

  const thumbs = useMemo(
    () =>
      items.map(item => {
        const type = inferType(item.src)
        const isVideo = type !== "image"
        const thumbSrc =
          type === "youtube"
            ? (inferPoster(item.src) ?? item.poster)
            : (item.poster ?? (isVideo ? inferPoster(item.src) : item.src))
        return { src: thumbSrc, isVideo }
      }),
    [items],
  )

  if (!items.length) {
    return <div className="h-[416px] max-w-[750px] rounded-lg bg-gray-100" />
  }

  return (
    <div className="flex max-w-[750px] flex-col gap-3">
      <div className="relative h-[416px] overflow-hidden rounded-lg bg-white">
        <Swiper
          className="size-full"
          effect="fade"
          modules={[EffectFade, Navigation, Pagination]}
          navigation={{
            nextEl: ".swiper-button-next",
            prevEl: ".swiper-button-prev",
          }}
          pagination={{
            clickable: true,
            // simple dots without extra labels
            renderBullet: (_: number, className: string) => `<div class="${className}"></div>`,
          }}
          onSwiper={setSwiper}
          onSlideChange={s => setActiveIndex(s.activeIndex)}
        >
          {items.map((item, index) => (
            <SwiperSlide key={String(item.id ?? item.src ?? index)}>
              <SlideContent item={item} isActive={index === activeIndex} />
            </SwiperSlide>
          ))}
        </Swiper>

        <div className="swiper-button-prev absolute left-2 top-1/2 z-10 flex size-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-black/10">
          <SvgChevronLeftMd className="stroke-white" />
        </div>
        <div className="swiper-button-next absolute right-2 top-1/2 z-10 flex size-11 -translate-y-1/2 items-center justify-center rounded-full px-3 hover:bg-black/10">
          <SvgChevronRightMd className="stroke-white" />
        </div>
      </div>

      {thumbs.length > 1 ? (
        <div className="flex gap-2 overflow-auto">
          {thumbs.map((t, idx) => (
            <button
              key={`${t.src}-${idx}`}
              type="button"
              onClick={() => swiper?.slideTo?.(idx)}
              className={twMerge(
                "h-15 w-24 shrink-0 overflow-hidden rounded-md outline-none focus:outline-none focus-visible:outline-none",
                idx !== activeIndex && "opacity-60 hover:opacity-100",
              )}
            >
              <div className="relative size-full">
                {t.src ? (
                  <img src={t.src} alt="" className="size-full object-cover" />
                ) : (
                  <div className="h-15 w-24 bg-black/30" />
                )}
                {t.isVideo ? <VideoPlayOverlay /> : null}
              </div>
            </button>
          ))}
        </div>
      ) : null}
    </div>
  )
})
