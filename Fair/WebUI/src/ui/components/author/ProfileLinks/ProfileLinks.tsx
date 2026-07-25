import { memo } from "react"
import { TFunction } from "i18next"

import { SvgPersonSquare } from "assets"

import { ProfileLink } from "./ProfileLink"
import { getSocialIconFromLink, getSocialNameFromLink } from "./utils"

export type ProfileLinksProps = {
  t: TFunction
  links: string[]
  authorLink?: string
}

export const ProfileLinks = memo(({ t, links, authorLink }: ProfileLinksProps) => (
  <div className="flex flex-wrap gap-2">
    {authorLink && (
      <ProfileLink
        link={authorLink}
        icon={<SvgPersonSquare className="fill-gray-800" />}
        text={t("common:globalProfile")}
        className="capitalize"
      />
    )}
    {links.map(x => {
      const icon = getSocialIconFromLink(x)
      const text = getSocialNameFromLink(t, x) ?? t("common:officialStore")
      return <ProfileLink key={x} link={x} icon={icon} text={text} className="capitalize" />
    })}
  </div>
))
