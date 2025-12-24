import { memo } from "react"

import avatarFallback from "assets/fallback/site-logo-3xl.png"
import { buildFileUrl } from "utils"

export type LogoDropdownButtonProps = {
  title: string
  imageFileId?: string
}

export const LogoDropdownButton = memo(({ title, imageFileId }: LogoDropdownButtonProps) => {
  // const [isExpanded, setExpanded] = useState(false)

  // const { context, /* floatingStyles,*/ refs } = useFloating({
  //   middleware: [offset(4)],
  //   open: isExpanded,
  //   placement: "bottom-end",
  //   onOpenChange: setExpanded,
  // })

  // const dismiss = useDismiss(context)
  // const role = useRole(context)
  // const hover = useHover(context, { handleClose: safePolygon() })
  // const { getReferenceProps /* getFloatingProps */ } = useInteractions([dismiss, role, hover])

  // useEffect(() => {
  //   const handler = () => setExpanded(false)
  //   window.addEventListener("resize", handler)
  //   window.addEventListener("scroll", handler, true)
  //   return () => {
  //     window.removeEventListener("resize", handler)
  //     window.removeEventListener("scroll", handler, true)
  //   }
  // }, [])

  return (
    <div
      //ref={refs.setReference}
      className="flex cursor-pointer items-center rounded-xl p-1 hover:bg-gray-100"
      title={title}
      //{...getReferenceProps()}
    >
      <div className="flex select-none items-center gap-3">
        <div className="size-10 overflow-hidden rounded-lg">
          <img
            src={imageFileId ? buildFileUrl(imageFileId) : avatarFallback}
            alt="Logo"
            className="size-full object-contain object-center"
            loading="lazy"
            onError={e => {
              e.currentTarget.onerror = null
              e.currentTarget.src = avatarFallback
            }}
          />
        </div>
        <span className="w-21.5 truncate text-2base font-medium leading-5.25 text-gray-800">{title}</span>
      </div>
      {/* <DropdownButton expanded={isExpanded} />
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu
            items={[
              { label: "abc", to: "/abc" },
              { label: "cde", to: "/cde" },
            ]}
            ref={refs.setFloating}
            style={floatingStyles}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )} */}
    </div>
  )
})
