import { createContext, useState, useContext, PropsWithChildren } from "react"

type SearchQueryContextType = {
  query: string
  setQuery: (value: string) => void
  triggerSearchEvent: () => void
  onSearchEvent: (callback: () => void) => () => void
}

const SearchQueryContext = createContext<SearchQueryContextType | undefined>(undefined)

export const SearchQueryProvider = ({ children }: PropsWithChildren) => {
  const [query, setQuery] = useState("")
  const [subscribers, setSubscribers] = useState<(() => void)[]>([])

  const triggerSearchEvent = () => {
    subscribers.forEach(callback => callback())
  }

  const onSearchEvent = (callback: () => void) => {
    setSubscribers(prev => [...prev, callback])
    return () => {
      setSubscribers(prev => prev.filter(cb => cb !== callback))
    }
  }

  return (
    <SearchQueryContext.Provider value={{ query, setQuery, triggerSearchEvent, onSearchEvent }}>
      {children}
    </SearchQueryContext.Provider>
  )
}

export const useSearchQueryContext = () => {
  const context = useContext(SearchQueryContext)
  if (!context) {
    throw new Error("useSearchQueryContext must be used within an SearchQueryProvider")
  }
  return context
}
