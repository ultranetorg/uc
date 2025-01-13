import { useQuery } from "@tanstack/react-query"
import { getApi } from "api"

export const useGetWeather = () => {
  const api = getApi()

  const queryFn = () => {
    return api.getWeathers()
  }

  const { isPending, error, data } = useQuery({
    queryKey: ["weathers"],
    queryFn: queryFn,
  })

  return { isPending, error, data }
}
