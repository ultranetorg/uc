import { TFunction } from "i18next"

import { formatDuration } from "./formatUtils"

export const getLastsFor = (t: TFunction, creationTime: number): string => {
  const nowSeconds = (Date.now() - Date.UTC(2026, 0, 1)) / 1000
  const hoursDuration = Math.max(0, nowSeconds - creationTime) / 3600
  return formatDuration(t, hoursDuration)
}
