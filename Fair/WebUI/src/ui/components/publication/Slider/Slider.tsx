import { Swiper, SwiperSlide } from "swiper/react"
import { EffectFade, Navigation, Pagination } from "swiper/modules"

import "swiper/css"
import "swiper/css/effect-fade"
import "swiper/css/navigation"
import "swiper/css/pagination"

import "./styles.css"
import { SvgChevronLeftMd, SvgChevronRightMd } from "assets"

export const Slider = () => {
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
        <SwiperSlide />
        <SwiperSlide />
        <SwiperSlide />
        <SwiperSlide />

        <div className="swiper-button-prev transition-base ml-1 h-11 w-11 rounded-full px-3 hover:bg-white/10">
          <SvgChevronLeftMd className="stroke-white" />
        </div>
        <div className="swiper-button-next transition-base mr-1 h-11 w-11 rounded-full px-3 hover:bg-white/10">
          <SvgChevronRightMd className="stroke-white" />
        </div>
      </Swiper>
    </div>
  )
}
