import { createContext, useContext } from "react"
import type { AccountBase } from "types"

export const PublicationOwnerContext = createContext<AccountBase | undefined>(undefined)
export const usePublicationOwner = () => useContext(PublicationOwnerContext)
