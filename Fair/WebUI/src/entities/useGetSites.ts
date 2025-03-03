import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetSites = () => {
  const queryFn = () => {
    return api.getSites()
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites"],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
