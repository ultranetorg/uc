import { Swiper, SwiperSlide } from "swiper/react"
import { EffectFade, Navigation, Pagination } from "swiper/modules"

import "swiper/css"
import "swiper/css/effect-fade"
import "swiper/css/navigation"
import "swiper/css/pagination"

import "./styles.css"
import { SvgChevronLeftMd, SvgChevronRightMd } from "assets"

import { VideoViewPlain, VideoViewVk, VideoViewYouTube } from "ui/components"
import { getVideoType } from "utils"

export type SliderMediaItem = {
  src: string
  poster?: string
  alt?: string
}

export type SliderProps = {
  items?: SliderMediaItem[]
}

export const Slider = ({ items }: SliderProps) => {
  return (
    <div className="h-[416px] max-w-[750px] overflow-hidden rounded-xl bg-gradient-to-r from-[#0b1e3a] to-[#29593f] text-white">
      <Swiper
        className="h-full w-full"
        effect={"fade"}
        modules={[EffectFade, Navigation, Pagination]}
        navigation={{ nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" }}
        pagination={{
          clickable: true,
          renderBullet: (_: number, className: string) => `<div class="${className}"></div>`,
        }}
      >
        {(items ?? []).length > 0 ? (
          (items ?? []).map((item, idx) => {
            const type = getVideoType(item.src)

            return (
              <SwiperSlide key={idx}>
                {type === "youtube" ? (
                  <VideoViewYouTube url={item.src} className="h-full w-full" />
                ) : type === "vkVideo" ? (
                  <VideoViewVk url={item.src} className="h-full w-full" />
                ) : type === "video" ? (
                  <VideoViewPlain
                    url={item.src}
                    posterUrl={item.poster}
                    controls
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <img
                    src={item.src}
                    alt={item.alt ?? "slide"}
                    className="h-full w-full object-cover"
                    loading="lazy"
                  />
                )}
              </SwiperSlide>
            )
          })
        ) : (
          <SwiperSlide />
        )}

        <div className="swiper-button-prev ml-1 h-11 w-11 rounded-full px-3 hover:bg-white/10">
          <SvgChevronLeftMd className="stroke-white" />
        </div>
        <div className="swiper-button-next mr-1 h-11 w-11 rounded-full px-3 hover:bg-white/10">
          <SvgChevronRightMd className="stroke-white" />
        </div>
      </Swiper>
    </div>
  )
}
