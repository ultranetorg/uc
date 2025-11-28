import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"
import { useParams } from "react-router-dom"

import { useGetAccountByAddress, useGetCategories, useGetSite } from "entities"
import { CategoryParentBaseWithChildren, Site } from "types"
import { buildCategoryTree } from "utils"

const { VITE_APP_ACCOUNT_ADDRESS: ACCOUNT_ADDRESS } = import.meta.env

type RootContextType = {
  accountAddress?: string
  id?: string
  nickname?: string
  isAuthor?: boolean
  isModerator?: boolean
  isPending: boolean
  site?: Site
  error?: Error
  isCategoriesPending: boolean
  categories?: CategoryParentBaseWithChildren[]
}

const RootContext = createContext<RootContextType>({
  isPending: false,
  isCategoriesPending: false,
})

export const useRootContext = () => useContext(RootContext)

export const RootProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()

  const [accountAddress, setAccountAddress] = useState<string | undefined>(ACCOUNT_ADDRESS)
  const [id, setId] = useState<string | undefined>()
  const [nickname, setNickname] = useState<string | undefined>()

  const { data: account } = useGetAccountByAddress(accountAddress)
  const { data: site, isPending, error } = useGetSite(siteId)
  const { data: categories, isPending: isCategoriesPending } = useGetCategories(siteId, 2)

  const categoriesTree = useMemo(() => categories && buildCategoryTree(categories), [categories])

  const value = useMemo<RootContextType>(() => {
    return {
      accountAddress: ACCOUNT_ADDRESS,
      id,
      nickname: nickname || undefined,
      isAuthor: !!(id && site?.authorsIds.includes(id)),
      isModerator: !!(id && site?.moderatorsIds.includes(id)),
      isPending,
      site,
      error,
      isCategoriesPending,
      categories: categoriesTree,
    }
  }, [categoriesTree, error, id, isCategoriesPending, isPending, nickname, site])

  useEffect(() => {
    if (account) {
      setId(account.id)
      setNickname(account.nickname)
    }
  }, [account])

  return <RootContext.Provider value={value}>{children}</RootContext.Provider>
}
