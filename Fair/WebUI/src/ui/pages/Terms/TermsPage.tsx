import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { Breakpoints } from "constants"
import { useMediaQuery } from "hooks"
import { ScrollToTopButton, TableOfContent } from "ui/components"

import "./TermsPage.css"

export const TermsPage = () => {
  const { t } = useTranslation("terms")
  useDocumentTitle(t("title"))

  const isSmall = useMediaQuery(Breakpoints.sm)

  const [isScrolled, setScrolled] = useState(false)

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 80)
    }

    window.addEventListener("scroll", handleScroll)
    return () => window.removeEventListener("scroll", handleScroll)
  }, [])

  return (
    <div className="mt-10 flex w-full flex-col gap-4 sm:flex-row">
      <TableOfContent
        className="static order-1 w-full p-4 sm:sticky sm:top-14 sm:order-2 sm:w-1/3 sm:self-start"
        title={t("tableOfContent")}
      />
      <div
        className="order-2 flex-1 sm:order-1 sm:w-2/3"
        dangerouslySetInnerHTML={{
          __html: t("text", { interpolation: { escapeValue: false } }),
        }}
      />
      <ScrollToTopButton visible={isSmall && isScrolled} />
    </div>
  )
}
