import { useMatches } from "react-router-dom"

export const useHandleParam = (): any => {
  const matches = useMatches()
  return (
    matches
      .filter(match => !!match.handle)
      .map(match => match.handle)
      // @ts-ignore
      .at(0) ?? {}
  )
}
