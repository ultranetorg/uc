export const shortenWideString = (str: string) => {
  if (str.length > 8 && window.innerWidth < 640) {
    return `${str.slice(0, 6)}...${str.slice(str.length - 4, str.length)}`
  }

  return str
}

export const shortenString = (address?: string) =>
  address && `${address.slice(0, 6)}...${address.slice(address.length - 4, address.length)}`

export const shortenAddress = (address: string) => {
  return window.innerWidth >= 640
    ? `${address.slice(0, 32)}...${address.slice(address.length - 14, address.length)}`
    : `${address.slice(0, 6)}...${address.slice(address.length - 4, address.length)}`
}
