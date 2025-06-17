import { createContext, useState, useContext, PropsWithChildren } from "react"

type SearchQueryContextType = {
  query: string
  setQuery: (value: string) => void
}

const SearchQueryContext = createContext<SearchQueryContextType | undefined>(undefined)

export const SearchQueryProvider = ({ children }: PropsWithChildren) => {
  const [query, setQuery] = useState("")

  return <SearchQueryContext.Provider value={{ query, setQuery }}>{children}</SearchQueryContext.Provider>
}

export const useSearchQueryContext = () => {
  const context = useContext(SearchQueryContext)
  if (!context) {
    throw new Error("useSearchQueryContext must be used within an SearchQueryProvider")
  }
  return context
}
