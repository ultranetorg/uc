export const shortenAddress = (address: string): string => {
  const firstPart = address.slice(0, 10)
  const lastPart = address.slice(-10)
  return `${firstPart}...${lastPart}`
}
