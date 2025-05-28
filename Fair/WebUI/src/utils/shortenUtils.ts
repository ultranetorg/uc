export const shortenAddress = (address: string): string => {
  const firstPart = address.slice(2, 6)
  const lastPart = address.slice(-4)
  return `0x${firstPart}...${lastPart}`
}
