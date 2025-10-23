import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetFile = (id: string) => {
  const queryFn = () => api.getFile(id)

  const { isPending, error, data } = useQuery({
    queryKey: ["files", id],
    queryFn: queryFn,
  })

  return { isPending, error: error ?? undefined, data }
}
