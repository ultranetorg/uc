export const combineUrl = (baseUrl: string, path: string) => {
  const url = new URL(path, baseUrl)
  return url.toString()
}

export const isAbsoluteUrl = (url: string): boolean => url.startsWith("http://") || url.startsWith("https://")
