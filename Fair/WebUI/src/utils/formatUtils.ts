const THREE_DOTS = "..."
const OS_DIVIDER = " | "

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
