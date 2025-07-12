import { memo } from "react"

import { AuthorLink } from "types"

import { ProfileLink } from "./ProfileLink"

export type ProfileLinksProps = {
  links: AuthorLink[]
}

export const ProfileLinks = memo(({ links }: ProfileLinksProps) => (
  <div className="flex flex-wrap gap-2">
    {links.map(x => (
      <ProfileLink key={x.link} {...x} />
    ))}
  </div>
))
