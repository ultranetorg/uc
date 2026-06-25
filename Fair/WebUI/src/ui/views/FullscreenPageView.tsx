import { memo, PropsWithChildren, useCallback } from "react"
import { useLocation, useNavigate } from "react-router-dom"

import { SvgProfilePageClose } from "assets"
import { useEscapeKey } from "hooks"
import { routes } from "utils"

export const FullscreenPageView = memo(({ children }: PropsWithChildren) => {
  const location = useLocation()
  const navigate = useNavigate()

  const state = location.state as { backgroundLocation?: Location; defaultTabKey?: string } | undefined

  const handleClick = useCallback(() => {
    if (state?.backgroundLocation) navigate(-1)
    else navigate(routes.home())
  }, [navigate, state?.backgroundLocation])

  useEscapeKey(handleClick)

  return (
    <div className="fixed inset-0 z-50 bg-white">
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
