import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { FloatingTree } from "@floating-ui/react"
import { ToastContainer } from "react-toastify"

import { Router, SearchQueryProvider } from "./app"

import "./i18n"
import "./styles/index.css"

// Все запросы выполняются только явно: первичная загрузка при монтировании (с учётом enabled)
// и ручные refetch() / invalidateQueries(). Любые неявные фоновые обновления отключены.
// Опрос по интервалу задаётся точечно в конкретном хуке через refetchInterval.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: Infinity,
      refetchOnMount: false,
      refetchOnWindowFocus: false,
      refetchOnReconnect: false,
      refetchInterval: false,
      retry: false,
    },
  },
})

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <SearchQueryProvider>
        <FloatingTree>
          <Router />
          <ToastContainer
            position="bottom-right"
            autoClose={5000}
            hideProgressBar={true}
            closeButton={false}
            pauseOnHover
            toastStyle={{ background: "transparent", boxShadow: "none", padding: 0 }}
          />
        </FloatingTree>
      </SearchQueryProvider>
    </QueryClientProvider>
  </StrictMode>,
)
