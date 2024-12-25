const utcNow = (): Date => {
  const date = new Date()
  const utcMilliseconds = Date.UTC(
    date.getUTCFullYear(),
    date.getUTCMonth(),
    date.getUTCDate(),
    date.getUTCHours(),
    date.getUTCMinutes(),
    date.getUTCSeconds(),
  )
  return new Date(utcMilliseconds)
}

export const daysToDate = (days: number): Date => {
  if (days > 0) {
    const date = new Date(Date.UTC(2020, 0, 1))
    date.setDate(date.getDate() + days)
    return date
  }

  return utcNow()
}
