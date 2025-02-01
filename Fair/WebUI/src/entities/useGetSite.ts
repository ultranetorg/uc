import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSite = () => {
  const queryFn = () => {
    return api.getSite()
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", ""],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
