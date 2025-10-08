import { createContext, PropsWithChildren, useCallback, useContext, useMemo, useState } from "react"
import { useParams, useSearchParams } from "react-router-dom"
import { FormProvider, useForm } from "react-hook-form"

import { CREATE_PROPOSAL_DURATION_DEFAULT } from "constants/"
import { useGetCategories } from "entities"
import { CategoryParentBaseWithChildren, CreateProposalData, OperationType } from "types"
import { MembersChangeModal } from "ui/components/proposal"
import { buildCategoryTree } from "utils"

type ModerationContextType = {
  lastEditedOptionIndex?: number
  setLastEditedOptionIndex: (index: number) => void
  isCategoriesPending: boolean
  refetchCategories: () => void
  categories?: CategoryParentBaseWithChildren[]
  openMembersChangeModal: () => void
}

// @ts-expect-error createContext with default value
const ModerationContext = createContext<ModerationContextType>({
  isCategoriesPending: false,
})

export const useModerationContext = () => useContext(ModerationContext)

export const ModerationProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const [searchParams] = useSearchParams()

  const methods = useForm<CreateProposalData>({
    mode: "onChange",
    defaultValues: {
      title: "",
      options: [],
      duration: CREATE_PROPOSAL_DURATION_DEFAULT,
      ...(searchParams.get("type") && { type: searchParams.get("type")! as OperationType }),
      ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
      ...(searchParams.get("publicationId") && { publicationId: searchParams.get("publicationId")! }),
      ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
      ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
    },
    shouldUnregister: false,
  })

  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)
  const [lastEditedOptionIndex, setLastEditedOptionIndex] = useState<number | undefined>()

  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const openMembersChangeModal = useCallback(() => setMembersChangeModalOpen(true), [])

  const value = useMemo(
    () => ({
      lastEditedOptionIndex,
      setLastEditedOptionIndex,
      isCategoriesPending,
      refetchCategories,
      categories: categories && buildCategoryTree(categories),
      openMembersChangeModal,
    }),
    [lastEditedOptionIndex, isCategoriesPending, refetchCategories, categories, openMembersChangeModal],
  )

  const handleMembersChangeModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <ModerationContext.Provider value={value}>
      <FormProvider {...methods}>
        {children}
        {isMembersChangeModalOpen && <MembersChangeModal onClose={handleMembersChangeModalClose} />}
      </FormProvider>
    </ModerationContext.Provider>
  )
}
