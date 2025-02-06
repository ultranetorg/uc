import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const { VITE_APP_SITE_ID: SITE_ID } = import.meta.env

const api = getApi()

export const useGetSite = () => {
  const queryFn = () => {
    return api.getSite(SITE_ID)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["sites", SITE_ID],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
