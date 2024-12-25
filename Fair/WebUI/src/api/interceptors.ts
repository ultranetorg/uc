import { daysToDate } from "utils"

export const interceptDates = (data?: any) => {
  if (!data || typeof data !== "object") {
    return
  }

  for (const key of Object.keys(data)) {
    const value = data[key]

    if (typeof value === "number" && (key.toLowerCase().endsWith("day") || key.toLowerCase().endsWith("time"))) {
      data[key] = daysToDate(value)
    } else if (typeof value === "object") {
      interceptDates(value)
    }
  }
}
