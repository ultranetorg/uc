import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { FloatingTree } from "@floating-ui/react"

import { Router, SearchQueryProvider } from "./app"
import "./i18n"

import "./styles/index.css"

const queryClient = new QueryClient()

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <SearchQueryProvider>
        <FloatingTree>
          <Router />
        </FloatingTree>
      </SearchQueryProvider>
    </QueryClientProvider>
  </StrictMode>,
)
