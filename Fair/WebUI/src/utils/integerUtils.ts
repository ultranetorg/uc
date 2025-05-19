export const isInteger = (value?: string | null): boolean => {
  if (value === null || value === undefined) {
    return false
  }

  const num = Number(value)
  return Number.isInteger(num) && String(num) === value.trim()
}

export const parseInteger = (value: string): number | undefined => {
  const parsed = parseInt(value)
  return !isNaN(parsed) ? parsed : undefined
}
