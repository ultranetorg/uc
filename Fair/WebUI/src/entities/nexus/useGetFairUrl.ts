import { useEffect, useRef, useState } from "react"

import { getNexusApi } from "api"

const api = getNexusApi()

export const useGetFairUrl = (baseUrl?: string) => {
  const [data, setData] = useState<string | undefined>()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<Error | undefined>()

  const hasFetched = useRef(false)

  useEffect(() => {
    if (!baseUrl || hasFetched.current) return

    setIsLoading(true)

    api
      .getFairUrl(baseUrl)
      .then(res => setData(res?.api))
      .catch(err => setError(err))
      .finally(() => {
        setIsLoading(false)
        hasFetched.current = true
      })
  }, [baseUrl, isLoading])

  return { data, isLoading, error }
}
