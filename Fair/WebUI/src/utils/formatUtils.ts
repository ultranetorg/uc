import dayjs from "dayjs"
import { TFunction } from "i18next"
import { capitalize } from "lodash"

import { START_DATE } from "config"
import { BaseVotableOperation } from "types"

const OS_DIVIDER = " | "
const ROLES_DELIMITER = ", "
const SOFTWARE_DELIMITER = ", "
const THREE_DOTS = "..."
const VOTES_DELIMITER = " / "

export const formatAr = (t: TFunction, approved: number, rejected: number) =>
  `${t("common:approved")}: ${approved} / ${t("common:rejected")}: ${rejected}`

export const formatArShort = (approved: number, rejected: number) => `${approved} / ${rejected}`

export const formatNabb = (t: TFunction, neither: number, any: number, ban: number, banish: number) =>
  `${t("common:neither")}: ${neither} / ${t("common:any")}: ${any} / ${t("common:ban")}: ${ban} / ${t("common:banish")}: ${banish}`

export const formatNabbShort = (neither: number, any: number, ban: number, banish: number) =>
  `${neither} / ${any} / ${ban} / ${banish}`

export const formatAverageRating = (value: number): string => (value / 10).toFixed(1)

export const formatDate = (hours: number): string =>
  dayjs(START_DATE).add(hours, "hour").startOf("day").format("DD.MM.YYYY")

export const formatDaysLeft = (createdAt: number, hoursLeft: number): number => Math.ceil((createdAt - hoursLeft) / 24)

export const formatSupportedPlatforms = (platforms: string[]): string => platforms.join(" / ")

export const formatUiLanguages = (languages: string[]): string => languages.join(", ")

export function formatSecDate(seconds: number) {
  return dayjs(START_DATE).add(seconds, "seconds").startOf("day").format("DD.MM.YYYY")
}

export const formatDuration = (t: TFunction, durationInHours: number): string => {
  const HOURS_IN_DAY = 24
  const HOURS_IN_MONTH = 30 * HOURS_IN_DAY
  const HOURS_IN_YEAR = 365 * HOURS_IN_DAY

  if (durationInHours >= HOURS_IN_YEAR) {
    const years = Math.floor(durationInHours / HOURS_IN_YEAR)
    const months = Math.floor((durationInHours - years * HOURS_IN_YEAR) / HOURS_IN_MONTH)
    return months > 0
      ? t("date:year", { count: years }) + " " + t("date:month", { count: months })
      : t("date:year", { count: years })
  }

  if (durationInHours >= 31 * HOURS_IN_DAY) {
    const months = Math.floor(durationInHours / HOURS_IN_MONTH)
    const days = Math.floor((durationInHours - months * HOURS_IN_MONTH) / HOURS_IN_DAY)
    return days > 0
      ? t("date:month", { count: months }) + " " + t("date:day", { count: days })
      : t("date:month", { count: months })
  }

  if (durationInHours >= HOURS_IN_DAY) {
    const days = Math.floor(durationInHours / HOURS_IN_DAY)
    const hours = Math.floor(durationInHours - days * HOURS_IN_DAY)
    return hours > 0
      ? t("date:day", { count: days }) + " " + t("date:hour", { count: hours })
      : t("date:day", { count: days })
  }

  return t("date:hour", { count: Math.floor(durationInHours) })
}

export const formatOption = (option: BaseVotableOperation, t: TFunction) => {
  return t(`operations:${option.$type}`)
}

export const formatOSes = (oses: string[], maxItems: number = 3): string =>
  oses.length > maxItems ? oses.slice(0, maxItems).join(OS_DIVIDER) + " " + THREE_DOTS : oses.join(OS_DIVIDER)

export const formatPercents = (value?: number): string | undefined => (value !== undefined ? `${value} %` : undefined)

export const formatRoles = (categories: string[]) => categories.map(capitalize).join(ROLES_DELIMITER)

export const formatSoftwareCategories = (categories: string[]) => categories.join(SOFTWARE_DELIMITER)

export const formatTitle = (title: string, maxLength: number = 48, endLength: number = 16): string => {
  if (title.length <= maxLength) {
    return title
  }

  const availableStartLength = maxLength - endLength - THREE_DOTS.length // Minus "..."
  const start = title.slice(0, availableStartLength)
  const end = title.slice(-endLength)
  return `${start} ${THREE_DOTS} ${end}`
}

export const formatVotes = (votes: number[]): string => votes.join(VOTES_DELIMITER)

export function ensureHttp(uri: string) {
  if (/^(https?:)?\/\//i.test(uri)) return uri
  return `https://${uri}`
}
