import React from "react"
import type { AccountBase } from "types"

import { PublicationOwnerContext } from "./context.ts"

export const PublicationOwnerProvider = ({ owner, children }: { owner?: AccountBase; children: React.ReactNode }) => {
  return <PublicationOwnerContext.Provider value={owner}>{children}</PublicationOwnerContext.Provider>
}

export default PublicationOwnerProvider
