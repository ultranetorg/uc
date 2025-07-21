import { TFunction } from "i18next"

import { BaseVotableOperation, SiteNicknameChange } from "types"

const THREE_DOTS = "..."
const OS_DIVIDER = " | "
const SOFTWARE_DELIMITER = ", "
const ROLES_DELIMITER = ", "

export const formatAverageRating = (value: number): string => (value / 10).toFixed(1)

export const formatTitle = (title: string, maxLength: number = 48, endLength: number = 16): string => {
  if (title.length <= maxLength) {
    return title
  }

  const availableStartLength = maxLength - endLength - THREE_DOTS.length // Minus "..."
  const start = title.slice(0, availableStartLength)
  const end = title.slice(-endLength)
  return `${start} ${THREE_DOTS} ${end}`
}

export const formatOSes = (oses: string[], maxItems: number = 3): string =>
  oses.length > maxItems ? oses.slice(0, maxItems).join(OS_DIVIDER) + " " + THREE_DOTS : oses.join(OS_DIVIDER)

export const formatSoftwareCategories = (categories: string[]) => categories.join(SOFTWARE_DELIMITER)

export const formatRoles = (categories: string[]) => categories.join(ROLES_DELIMITER)

export const formatOption = (option: BaseVotableOperation, t: TFunction) => {
  return t(`operations:${option.$type}`)
}
