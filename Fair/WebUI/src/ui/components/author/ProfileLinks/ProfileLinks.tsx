import { memo } from "react"
import { TFunction } from "i18next"

import { ProfileLink } from "./ProfileLink"
import { getSocialIconFromLink, getSocialNameFromLink } from "./utils"

export type ProfileLinksProps = {
  t: TFunction
  links: string[]
}

export const ProfileLinks = memo(({ t, links }: ProfileLinksProps) => (
  <div className="flex flex-wrap gap-2">
    {links.map(x => {
      const icon = getSocialIconFromLink(x)
      const text = getSocialNameFromLink(t, x) ?? t("official")
      return <ProfileLink key={x} link={x} icon={icon} text={text} />
    })}
  </div>
))
