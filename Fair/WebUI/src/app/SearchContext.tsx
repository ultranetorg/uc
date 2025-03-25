import { createContext, useState, useContext, PropsWithChildren } from "react"

type SearchContextType = {
  search: string
  setSearch: (value: string) => void
}

const SearchContext = createContext<SearchContextType | undefined>(undefined)

export const SearchProvider = ({ children }: PropsWithChildren) => {
  const [search, setSearch] = useState("")

  return <SearchContext.Provider value={{ search, setSearch }}>{children}</SearchContext.Provider>
}

export const useSearchContext = () => {
  const context = useContext(SearchContext)
  if (!context) {
    throw new Error("useSearchContext must be used within an SearchProvider")
  }
  return context
}
