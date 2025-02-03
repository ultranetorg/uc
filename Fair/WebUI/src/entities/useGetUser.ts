import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

const api = getApi()

export const useGetUser = (userId?: string) => {
  const queryFn = () => {
    if (!userId) {
      return
    }

    return api.getUser(userId)
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["users", userId],
    queryFn: queryFn,
    enabled: !!userId,
  })

  return { isPending, error, data }
}
