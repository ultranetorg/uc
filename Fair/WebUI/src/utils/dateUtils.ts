import dayjs from "dayjs"

export const formatDate = (days: number): string => {
  return dayjs("2025-01-01").add(days, "day").startOf("day").format("DD MMM YYYY")
}
