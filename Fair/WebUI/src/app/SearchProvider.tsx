import { createContext, useState, useContext, PropsWithChildren } from "react"

type SearchQueryContextType = {
  query: string
  setQuery: (value: string) => void
}

const SearchQueryContext = createContext<SearchQueryContextType>({
  query: "",
  setQuery: () => {},
})

export const SearchQueryProvider = ({ children }: PropsWithChildren) => {
  const [query, setQuery] = useState("")

  return <SearchQueryContext.Provider value={{ query, setQuery }}>{children}</SearchQueryContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSearchQueryContext = () => useContext(SearchQueryContext)
