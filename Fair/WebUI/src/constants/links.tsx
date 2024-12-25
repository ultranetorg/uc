import { FC, SVGProps } from "react"

import { SvgEnvelope, SvgFileText, SvgGithub, SvgSteam, SvgTelegram, SvgTwitterXCircleFill, SvgYoutube } from "assets"

type Link = {
  translationKey: string
  url: string
  Icon: FC<SVGProps<SVGSVGElement>>
}

export const socialLinks: Link[] = [
  {
    translationKey: "github",
    url: "https://github.com/ultranetorg",
    Icon: props => <SvgGithub {...props} />,
  },
  {
    translationKey: "telegram",
    url: "https://t.me/ultranetchat",
    Icon: props => <SvgTelegram {...props} />,
  },
  {
    translationKey: "twitter_x",
    url: "https://twitter.com/ultranetorg",
    Icon: props => <SvgTwitterXCircleFill {...props} />,
  },
  {
    translationKey: "youtube",
    url: "https://www.youtube.com/channel/UCauxo1yuwGkr_D4N1DxaYUg",
    Icon: props => <SvgYoutube {...props} />,
  },
  {
    translationKey: "steam",
    url: "https://store.steampowered.com/app/1422050",
    Icon: props => <SvgSteam {...props} />,
  },
]

export const links: Link[] = [
  {
    translationKey: "contactUs",
    url: "https://www.ultranet.org/contacts",
    Icon: props => <SvgEnvelope {...props} />,
  },
  {
    translationKey: "terms",
    url: "/terms",
    Icon: props => <SvgFileText {...props} />,
  },
]
