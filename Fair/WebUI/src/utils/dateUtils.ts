import dayjs from "dayjs"

import { START_DATE } from "config"

export const getHoursPassedFromStart = (): number => {
  const startDate = dayjs(START_DATE)
  return dayjs().diff(startDate, "hours")
}
