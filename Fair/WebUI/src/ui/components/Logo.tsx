import { memo, MouseEventHandler } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgUltranetExplorer, SvgUltranetExplorerColored } from "assets"

type LogoProps = {
  className?: string
  height?: number | undefined
  title?: string | undefined
  onClick?: MouseEventHandler<SVGSVGElement> | undefined
}

export const Logo = memo((props: LogoProps) => {
  const { className, title, onClick } = props

  const { t } = useTranslation("common")

  return (
    <>
      {onClick ? (
        <SvgUltranetExplorerColored
          className={twMerge(className, "cursor-pointer")}
          title={title || t("logo")}
          onClick={onClick}
        />
      ) : (
        <SvgUltranetExplorer className="fill-[#989898]" />
      )}
    </>
  )
})
