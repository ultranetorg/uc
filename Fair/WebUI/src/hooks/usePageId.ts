import { useMatches } from "react-router-dom"

export const usePageId = (): string => {
  const matches = useMatches()
  return (
    matches
      // @ts-ignore
      .filter(match => Boolean(match.handle?.pageId))
      // @ts-ignore
      .map(match => match.handle.pageId)
      // @ts-ignore
      .at(0) ?? ""
  )
}
