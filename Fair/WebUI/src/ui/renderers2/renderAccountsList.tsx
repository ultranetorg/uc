import { ReactNode } from "react"

import { AccountBase } from "types"
import { AccountInfo } from "ui/components"
import { shortenAddress } from "utils"

export const renderAccountsList = (accounts: AccountBase[]): ReactNode => {
  const accountsToRender = accounts.slice(0, 2)

  return (
    <div className="flex items-center gap-2">
      {accountsToRender.map(x => (
        <AccountInfo
          key={x.id}
          title={x.nickname || shortenAddress(x.address)}
          fullTitle={x.nickname || x.address}
          avatar={x.id}
        />
      ))}
    </div>
  )
}
