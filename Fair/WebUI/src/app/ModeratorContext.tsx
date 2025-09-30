import { createContext, PropsWithChildren, useCallback, useContext, useMemo, useState } from "react"
import { useParams } from "react-router-dom"

import { useGetCategories } from "entities"
import { CategoryParentBaseWithChildren } from "types"
import { MembersChangeModal } from "ui/components/proposal"
import { buildCategoryTree } from "utils"

type ModeratorContextType = {
  isCategoriesPending: boolean
  refetchCategories?: () => void
  categories?: CategoryParentBaseWithChildren[]
  openMembersChangeModal?: () => void
}

const ModeratorContext = createContext<ModeratorContextType>({
  isCategoriesPending: false,
})

export const useModeratorContext = () => useContext(ModeratorContext)

export const ModeratorProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()

  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)

  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const openMembersChangeModal = () => setMembersChangeModalOpen(true)

  const value = useMemo(
    () => ({
      isCategoriesPending,
      refetchCategories,
      categories: categories && buildCategoryTree(categories),
      openMembersChangeModal,
    }),
    [isCategoriesPending, refetchCategories, categories],
  )

  const handleMembersChangeModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <ModeratorContext.Provider value={value}>
      {children}
      {isMembersChangeModalOpen && <MembersChangeModal onClose={handleMembersChangeModalClose} />}
    </ModeratorContext.Provider>
  )
}
