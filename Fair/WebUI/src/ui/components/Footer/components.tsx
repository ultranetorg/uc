/* eslint-disable jsx-a11y/anchor-is-valid */

import { memo, useCallback, MouseEvent } from "react"
import { useNavigate } from "react-router"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useSettings } from "app/"
import { links, socialLinks } from "constants"
import { Currency, PropsWithClassName } from "types"
import { Logo, Switch } from "ui/components"
import { isAbsoluteUrl } from "utils"

export const CopyrightLogo = memo((props: PropsWithClassName) => {
  const { t } = useTranslation("footer")

  return (
    <div className={twMerge("flex w-64 flex-col items-center justify-between lg:items-start", props.className)}>
      <Logo />
      <div className="cursor-default text-center text-[13px] leading-[17px] text-[#989898] lg:text-start">
        {t("copyright")}
      </div>
    </div>
  )
})

export const LinksList = memo((props: PropsWithClassName) => {
  const { t } = useTranslation("footer")
  const navigate = useNavigate()

  const handleLinkClick = useCallback(
    (e: MouseEvent) => {
      e.preventDefault()

      const url = e.currentTarget.attributes.getNamedItem("data-url")?.value
      if (!url) {
        return
      }

      if (isAbsoluteUrl(url)) {
        window.location.href = url
      } else {
        window.scrollTo({ top: 0 })
        navigate(url)
      }
    },
    [navigate],
  )

  return (
    <div className={twMerge("flex h-full flex-col items-center justify-between", props.className)}>
      <div className="flex w-full justify-between">
        {socialLinks.map(link => (
          <div key={link.translationKey} title={t(link.translationKey)}>
            <link.Icon
              className="cursor-pointer fill-[#989898] hover:fill-[#3dc1f2]"
              data-url={link.url}
              onClick={handleLinkClick}
            />
          </div>
        ))}
      </div>
      <div className="flex justify-center gap-7">
        {links.map(link => (
          <div key={link.translationKey} title={t(link.translationKey)}>
            <a
              className="group flex gap-2 no-underline hover:no-underline"
              href="#"
              onClick={handleLinkClick}
              data-url={link.url}
            >
              <link.Icon className="cursor-pointer fill-[#989898] group-hover:fill-[#3dc1f2]" />
              <div className="text-sm leading-[14px] text-[#989898] group-hover:text-[#3dc1f2]">
                {t(link.translationKey)}
              </div>
            </a>
          </div>
        ))}
      </div>
    </div>
  )
})

export const PricesSwitch = memo((props: PropsWithClassName) => {
  const { currency, setCurrency } = useSettings()

  return (
    <div className={twMerge("flex w-64 justify-end", props.className)}>
      <Switch
        className="h-6 w-fit font-sans-medium text-sm leading-3"
        item1={{ label: "$", value: "USD", title: "USD" }}
        item2={{ label: "UNT", value: "UNT", title: "UNT" }}
        value={currency}
        onChange={value => setCurrency(value as Currency)}
      />
    </div>
  )
})
