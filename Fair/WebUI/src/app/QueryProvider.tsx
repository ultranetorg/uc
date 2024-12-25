import { memo, PropsWithChildren } from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"

export const QueryProvider = memo(({ children }: PropsWithChildren) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        refetchOnReconnect: true,
        refetchOnWindowFocus: false,
        throwOnError: true,
      },
    },
  })

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
})
