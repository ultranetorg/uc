import dayjs from "dayjs"

import { START_DATE } from "config"

export const getDaysPassedFromStart = (): number => {
  const startDate = dayjs(START_DATE)
  return dayjs().diff(startDate, "day")
}
