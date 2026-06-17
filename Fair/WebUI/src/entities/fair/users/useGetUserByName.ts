import { useQuery } from "@tanstack/react-query"

import { getFairApi } from "api"
import { USER_NAME_RANGE_REGEXP } from "utils"

import { usersKeys } from "./usersKeys"

const api = getFairApi()

export const useGetUserByName = (name?: string) => {
  const queryFn = () => api.getUser(name!)

  const { isFetching, isError, data } = useQuery({
    queryKey: usersKeys.info(name!),
    queryFn: queryFn,
    enabled: !!name && USER_NAME_RANGE_REGEXP.test(name),
  })

  return { isFetching, isError, data }
}
