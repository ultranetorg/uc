import { useCallback } from "react"
import { Link, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useHandleParam } from "hooks"
import { Container, NetworkDropdown, Logo } from "ui/components"

export const Navbar = () => {
  const navigate = useNavigate()
  const { t } = useTranslation("layout")
  const { isAuctionsPath } = useHandleParam()

  const handleLogoClick = useCallback(() => navigate("/"), [navigate])

  return (
    <div className="flex w-full bg-zinc-900 py-5">
      <Container>
        <div className="flex w-full flex-col items-center justify-between gap-4 xs:flex-row">
          <div>
            <Logo onClick={handleLogoClick} />
          </div>
          <div className="flex flex-col items-center gap-4 text-base leading-4 xs:flex-row">
            {isAuctionsPath !== true ? (
              <Link to={"/auctions"}>{t("auctions")}</Link>
            ) : (
              <Link to={"/"}>{t("network")}</Link>
            )}
            <NetworkDropdown />
          </div>
        </div>
      </Container>
    </div>
  )
}
