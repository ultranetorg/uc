import { ReactNode } from "react"

import {
  SvgDiscordColored,
  SvgFacebookColored,
  SvgGithubColored,
  SvgInstagramColored,
  SvgLinkedInColored,
  SvgMediumColored,
  SvgRedditColored,
  SvgTelegramColored,
  SvgTwitchColored,
  SvgXTwitterColored,
  SvgYoutubeColored,
} from "assets"
import { TFunction } from "i18next"

export const getSocialIconFromLink = (link: string): ReactNode => {
  if (link) {
    const lower = link.toLowerCase()
    if (lower.includes("discord.com")) return <SvgDiscordColored />
    if (lower.includes("facebook.com") || lower.includes("fb.com")) return <SvgFacebookColored />
    if (lower.includes("github.com")) return <SvgGithubColored />
    if (lower.includes("instagram.com")) return <SvgInstagramColored />
    if (lower.includes("linkedin.com")) return <SvgLinkedInColored />
    if (lower.includes("medium.com")) return <SvgMediumColored />
    if (lower.includes("reddit.com")) return <SvgRedditColored />
    if (lower.includes("telegram.com") || lower.includes("t.me")) return <SvgTelegramColored />
    if (lower.includes("twitch.com")) return <SvgTwitchColored />
    if (lower.includes("twitter.com") || lower.includes("x.com")) return <SvgXTwitterColored />
    if (lower.includes("youtube.com") || lower.includes("youtu.be")) return <SvgYoutubeColored />
  }

  return null
}

export const getSocialNameFromLink = (t: TFunction, link: string): string | undefined => {
  if (link) {
    const lower = link.toLowerCase()
    if (lower.includes("discord.com")) return t("social:discord")
    if (lower.includes("facebook.com")) return t("social:facebook")
    if (lower.includes("github.com")) return t("social:github")
    if (lower.includes("instagram.com")) return t("social:instagram")
    if (lower.includes("linkedin.com")) return t("social:linkedin")
    if (lower.includes("medium.com")) return t("social:medium")
    if (lower.includes("reddit.com")) return t("social:reddit")
    if (lower.includes("telegram.com") || lower.includes("t.me")) return t("social:telegram")
    if (lower.includes("twitch.com")) return t("social:twitch")
    if (lower.includes("twitter.com") || lower.includes("x.com")) return t("social:x")
    if (lower.includes("youtube.com") || lower.includes("youtu.be")) return t("social:youtube")
  }

  return undefined
}
