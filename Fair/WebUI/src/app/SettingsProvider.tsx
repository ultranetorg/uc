import { ReactNode, createContext, useContext } from "react"
import { useLocalStorage } from "usehooks-ts"

import { Currency } from "types"

type ContextType = {
  currency: Currency
  setCurrency: (currency: Currency) => void
}

export const SettingsContext = createContext<ContextType>({} as ContextType)

type SettingsProviderProps = {
  children?: ReactNode
}

export const SettingsProvider = ({ children }: SettingsProviderProps) => {
  const [currency, setCurrency] = useLocalStorage<Currency>("currency", "USD")

  return <SettingsContext.Provider value={{ currency, setCurrency }}>{children}</SettingsContext.Provider>
}

export const useSettings = () => {
  const { currency, setCurrency } = useContext(SettingsContext)
  return { currency, setCurrency }
}
