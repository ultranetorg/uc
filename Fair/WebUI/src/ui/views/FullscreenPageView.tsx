import { memo, PropsWithChildren } from "react"

import { SvgProfilePageClose } from "assets"
import { useCloseFullscreen, useEscapeKey } from "hooks"
import { routes } from "utils"

export const FullscreenPageView = memo(({ children }: PropsWithChildren) => {
  const handleClick = useCloseFullscreen(routes.home())

  useEscapeKey(handleClick)

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto bg-white [scrollbar-gutter:stable]">
      <div className="mx-auto max-w-[1240px]">
        <div className="flex pl-17">
          <div className="flex w-full gap-6">
            <div className="flex w-full flex-col gap-6 py-8">{children}</div>
            <div className="pt-7.5">
              <SvgProfilePageClose className="cursor-pointer" onClick={handleClick} />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
})
