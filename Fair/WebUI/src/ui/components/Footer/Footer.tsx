/* eslint-disable jsx-a11y/anchor-is-valid */

import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { Breakpoints } from "constants"
import { useMediaQuery } from "hooks"
import { PropsWithClassName } from "types"
import { Container } from "ui/components"

import { CopyrightLogo, LinksList, PricesSwitch } from "./components"

export const Footer = memo((props: PropsWithClassName) => {
  const { className } = props

  const isLarge = useMediaQuery(Breakpoints.lg)
  const isMedium = useMediaQuery(Breakpoints.md)

  return (
    <div className={twMerge("w-full bg-zinc-900", className)}>
      <Container className="sm:px-20">
        {!isLarge && !isMedium ? (
          <div className="flex h-36 justify-between py-8">
            <CopyrightLogo className="w-64" />
            <LinksList className="w-64" />
            <PricesSwitch />
          </div>
        ) : !isMedium ? (
          <div className="flex flex-col gap-7 py-7">
            <div className="flex h-20 items-start justify-between">
              <LinksList className="w-64" />
              <PricesSwitch />
            </div>
            <div className="flex h-20 justify-center">
              <CopyrightLogo className="w-64" />
            </div>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-7 py-7">
            <PricesSwitch className="h-6 w-full justify-center" />
            <LinksList className="h-16 w-64 justify-between" />
            <CopyrightLogo className="h-20 w-64" />
          </div>
        )}
      </Container>
    </div>
  )
})
