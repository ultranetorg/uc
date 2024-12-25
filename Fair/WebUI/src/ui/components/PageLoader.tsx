import { twMerge } from "tailwind-merge"

import { SvgLoading } from "assets"
import { PropsWithClassName } from "types/common"

export const PageLoader = ({ className }: PropsWithClassName) => (
  <div className={twMerge("relative mx-auto mb-20 h-[25px] w-[25px] md:mb-60 md:max-w-[1200px]", className)}>
    <SvgLoading className="animate-spin" />
  </div>
)
