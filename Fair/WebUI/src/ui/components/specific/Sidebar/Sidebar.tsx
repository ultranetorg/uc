import { memo, useState } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import {
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useFloatingParentNodeId,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName, SiteBase } from "types"
import { SitesList } from "ui/components/sidebar"

import { AllSitesButton, CurrentAccountButton } from "./components"
import { AccountMenu } from "./AccountMenu"

const TEST_CURRENT_SITE: SiteBase = {
  id: "0",
  title: "GameNest",
}

const TEST_SITES: SiteBase[] = [
  { id: "1", title: "SoftVault" },
  { id: "2", title: "MovieMesh" },
  {
    id: "3",
    title:
      "This is very ery very very very ery very very very ery very very very ery very very very ery very very long site name",
  },
]

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const [isOpen, setIsOpen] = useState(false)

  const { t } = useTranslation("sites")

  const nodeId = useFloatingParentNodeId()
  const { context, floatingStyles, refs } = useFloating({
    nodeId: nodeId!,
    middleware: [offset(8)],
    open: isOpen,
    placement: "top-start",
    onOpenChange: setIsOpen,
  })

  const dismiss = useDismiss(context)
  const hover = useHover(context, { handleClose: safePolygon({ requireIntent: true }) })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

  return (
    <div className={twMerge("flex min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex flex-grow flex-col gap-8 p-2">
        <Link to="/">
          <AllSitesButton title={t("allSites")} />
        </Link>
        <SitesList title={t("currentSite")} items={[TEST_CURRENT_SITE]} emptyStateMessage={t("emptySitesList")} />
        <SitesList title={t("starredSites")} items={TEST_SITES} emptyStateMessage={t("emptySitesList")} />
      </div>
      <CurrentAccountButton
        nickname="This is very very long nickname"
        address="0xf2884A04A0caB3fa166c85DF55Ab1Af8549dB936"
        ref={refs.setReference}
        {...getReferenceProps()}
      />
      {isOpen && (
        <AccountMenu
          ref={refs.setFloating}
          style={floatingStyles}
          nickname="This is very very long nickname nickname nickname nickname nickname nickname nickname"
          address="0xf2884A04A0caB3fa166c85DF55Ab1Af8549dB936"
          {...getFloatingProps()}
        />
      )}
    </div>
  )
})
