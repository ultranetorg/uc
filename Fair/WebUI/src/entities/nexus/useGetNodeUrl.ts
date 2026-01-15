import { useEffect, useRef, useState } from "react"

import { getNexusApi } from "api"

const api = getNexusApi()

export const useGetNodeUrl = (baseUrl?: string) => {
  const [data, setData] = useState<string | undefined>()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<Error | undefined>()

  const hasFetched = useRef(false)

  useEffect(() => {
    if (isLoading || !baseUrl || hasFetched.current) return

    setIsLoading(true)

    api
      .getNodeUrl(baseUrl)
      .then(res => setData(res?.api))
      .catch(err => setError(err))
      .finally(() => {
        setIsLoading(false)
        hasFetched.current = true
      })
  }, [baseUrl, isLoading])

  return { data, isLoading, error }
}
