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
      {breadcrumbs.map((item, index) => (
        <>
          {"title" in item ? (
            <Breadcrumb2 title={item.title} path={index < breadcrumbs.length - 1 ? item.path : undefined} />
          ) : (
            <ThreeDotsSvg className="fill-gray-400" />
          )}
          {index < breadcrumbs.length - 1 && <SvgChevronRight className="stroke-gray-400" />}
        </>
      ))}
    </div>
  )
}
