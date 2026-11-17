import { memo, PropsWithChildren } from "react"
import { useRouteError } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgProfilePageClose2Xl } from "assets"
import { useCloseFullscreen, useEscapeKey } from "hooks"
import { routes } from "utils"

export const FullscreenPageView = memo(({ children }: PropsWithChildren) => {
  const error = useRouteError()
  const handleClick = useCloseFullscreen(routes.home())

  useEscapeKey(handleClick)

  return (
    <div className="fixed inset-0 z-50 scrollbar-gutter-stable overflow-y-auto bg-white">
      <div className={twMerge("mx-auto max-w-310", !!error && "h-full")}>
        <div className={twMerge("flex px-4 sm:pr-0 sm:pl-17", !!error && "h-full")}>
          <div className="flex w-full min-w-0 flex-col gap-2 sm:flex-row sm:gap-6">
            <div className="order-2 flex w-full min-w-0 flex-col gap-6 pb-6 sm:order-1 sm:py-8">{children}</div>
            <div className="order-1 flex shrink-0 justify-end pt-4 sm:order-2 sm:pt-7.5">
              <SvgProfilePageClose2Xl className="cursor-pointer" onClick={handleClick} />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
})
