import { useQuery } from "@tanstack/react-query"

import { getApi } from "api"

export const useGetResourceInboundLinks = (author: string, resource: string, page: number, pageSize?: number) => {
  const api = getApi()

  const queryFn = async () => {
    if (!author || !resource) {
      return
    }

    return await api.resources.getInboundLinks(author, resource, page, pageSize)
  }

  const { data, isLoading } = useQuery({
    queryKey: ["authors", author, "resources", resource, { page }],
    queryFn: queryFn,
    enabled: !!author && page > 0,
  })

  return {
    data,
    isLoading,
  }
}
