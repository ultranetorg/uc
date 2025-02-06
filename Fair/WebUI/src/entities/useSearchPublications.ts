import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const { VITE_APP_SITE_ID: SITE_ID } = import.meta.env

const api = getApi()

export const useSearchPublications = (name?: string, page?: number, pageSize?: number) => {
  const queryFn = () => {
    return api.searchPublications(SITE_ID, name, page, pageSize)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", SITE_ID, "publications", { name, page, pageSize }],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
