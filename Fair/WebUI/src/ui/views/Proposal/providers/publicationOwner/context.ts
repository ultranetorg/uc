import { createContext, useContext } from "react"
import type { AccountBaseAvatar } from "types"

export const PublicationOwnerContext = createContext<AccountBaseAvatar | undefined>(undefined)
export const usePublicationOwner = () => useContext(PublicationOwnerContext)
