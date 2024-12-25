import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetResource = (author?: string, name?: string) => {
  const api = getApi()

  const queryFn = async () => {
    if (!author || !name) {
      return
    }

    return await api.resources.getByAuthorName(author, name)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["authors", author, "resources", name],
    queryFn: queryFn,
    enabled: !!author && !!name,
  })

  return {
    data,
    isLoading,
  }
}
