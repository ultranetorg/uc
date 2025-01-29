import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useSearchPublications = (name?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    return api.searchPublications(name, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", "", "publications", { name, page, pageSize }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
