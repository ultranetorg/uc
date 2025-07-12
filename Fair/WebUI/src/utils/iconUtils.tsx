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

export const getSocialIconByLink = (link: string): ReactNode => {
  if (link) {
    const lower = link.toLowerCase()
    if (lower.includes("discord.com")) {
      return <SvgDiscordColored />
    }
    if (lower.includes("facebook.com") || lower.includes("fb.com")) {
      return <SvgFacebookColored />
    }
    if (lower.includes("github.com")) {
      return <SvgGithubColored />
    }
    if (lower.includes("instagram.com")) {
      return <SvgInstagramColored />
    }
    if (lower.includes("linkedin.com")) {
      return <SvgLinkedInColored />
    }
    if (lower.includes("medium.com")) {
      return <SvgMediumColored />
    }
    if (lower.includes("reddit.com")) {
      return <SvgRedditColored />
    }
    if (lower.includes("telegram.com") || lower.includes("t.me")) {
      return <SvgTelegramColored />
    }
    if (lower.includes("twitch.com")) {
      return <SvgTwitchColored />
    }
    if (lower.includes("x.com")) {
      return <SvgXTwitterColored />
    }
    if (lower.includes("youtube.com") || lower.includes("youtu.be")) {
      return <SvgYoutubeColored />
    }
  }

  return null
}
