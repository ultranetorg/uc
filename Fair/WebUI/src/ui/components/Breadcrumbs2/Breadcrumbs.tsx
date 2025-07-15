import { Fragment } from "react/jsx-runtime"

import { SvgChevronRight, ThreeDotsSvg } from "assets"
import { useAppBreadcrumbs } from "hooks"

import { Breadcrumb2 } from "./Breadcrumb"

export const Breadcrumbs2 = () => {
  const breadcrumbs = useAppBreadcrumbs()

  if (!breadcrumbs.length) {
    return null
  }

  return (
    <div className="flex h-6 select-none items-center gap-1 text-2xs leading-5">
      {breadcrumbs.map((item, index) => {
        const isLast = index === breadcrumbs.length - 1

        return (
          <Fragment key={index}>
            {"title" in item ? (
              <Breadcrumb2 title={item.title} path={!isLast ? item.path : undefined} />
            ) : (
              <ThreeDotsSvg className="fill-gray-400" />
            )}
            {!isLast && <SvgChevronRight className="stroke-gray-400" />}
          </Fragment>
        )
      })}
    </div>
  )
}
