import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"

import { Router, SearchQueryProvider } from "./app"
import "./i18n"

import "./index.css"

const queryClient = new QueryClient()

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <SearchQueryProvider>
        <Router />
      </SearchQueryProvider>
    </QueryClientProvider>
  </StrictMode>,
)
