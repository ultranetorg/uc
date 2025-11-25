import React from "react"
import type { AccountBaseAvatar } from "types"

import { PublicationOwnerContext } from "./context.ts"

export const PublicationOwnerProvider = ({
  owner,
  children,
}: {
  owner?: AccountBaseAvatar
  children: React.ReactNode
}) => {
  return <PublicationOwnerContext.Provider value={owner}>{children}</PublicationOwnerContext.Provider>
}

export default PublicationOwnerProvider
