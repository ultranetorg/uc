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

export const formatAnbb = (t: TFunction, any: number, neither: number, ban: number, banish: number) =>
  `${t("common:any")}: ${any} / ${t("common:neither")}: ${neither} / ${t("common:ban")}: ${ban} / ${t("common:banish")}: ${banish}`

export const formatAnbbShort = (any: number, neither: number, ban: number, banish: number) =>
  `${any} / ${neither} / ${ban} / ${banish}`

export const formatAverageRating = (value: number): string => (value / 10).toFixed(1)

export const formatDate = (days: number): string => {
  return dayjs(START_DATE).add(days, "day").startOf("day").format("DD MMM YYYY")
}

export function formatSecDate(seconds: number) {
  return dayjs(START_DATE).add(seconds, "seconds").startOf("day").format("DD MMM YYYY")
}

export const formatDuration = (t: TFunction, durationInDays: number): string => {
  const years = Math.floor(durationInDays / 365)
  if (years > 0) {
    const months = Math.floor((durationInDays - years * 365) / 30)
    return months > 0
      ? t("date:year", { count: years }) + " " + t("date:month", { count: months })
      : t("date:year", { count: years })
  }

  const months = Math.floor(durationInDays / 30)
  return months > 0 ? t("date:month", { count: months }) : t("date:day", { count: durationInDays })
}

export const formatOption = (option: BaseVotableOperation, t: TFunction) => {
  return t(`operations:${option.$type}`)
}

export const formatOSes = (oses: string[], maxItems: number = 3): string =>
  oses.length > maxItems ? oses.slice(0, maxItems).join(OS_DIVIDER) + " " + THREE_DOTS : oses.join(OS_DIVIDER)

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
