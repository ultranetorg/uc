import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"
import { USER_NAME_RANGE_REGEXP } from "utils"

const api = getApi()

export const useGetUser = (name?: string) => {
  const queryFn = () => api.getUser(name!)

  const { isFetching, isError, data } = useQuery({
    queryKey: ["users", name],
    queryFn: queryFn,
    enabled: !!name && USER_NAME_RANGE_REGEXP.test(name),
  })

  return { isFetching, isError, data }
}
