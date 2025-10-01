import {
  createContext,
  Dispatch,
  PropsWithChildren,
  SetStateAction,
  useCallback,
  useContext,
  useMemo,
  useState,
} from "react"
import { useParams, useSearchParams } from "react-router-dom"

import { CREATE_PROPOSAL_DURATION_DEFAULT } from "constants/"
import { useGetCategories } from "entities"
import { CategoryParentBaseWithChildren, CreateProposalData, CreateProposalDataOption, OperationType } from "types"
import { MembersChangeModal } from "ui/components/proposal"
import { buildCategoryTree } from "utils"

type ModerationContextType = {
  data: CreateProposalData
  setData: Dispatch<SetStateAction<CreateProposalData>>
  setDataOption: (index: number, setState: (prevState: CreateProposalDataOption) => CreateProposalDataOption) => void
  isCategoriesPending: boolean
  refetchCategories?: () => void
  categories?: CategoryParentBaseWithChildren[]
  openMembersChangeModal?: () => void
}

const ModerationContext = createContext<ModerationContextType>({
  data: { title: "", duration: CREATE_PROPOSAL_DURATION_DEFAULT },
  setData: () => {},
  setDataOption: () => {},
  isCategoriesPending: false,
})

export const useModerationContext = () => useContext(ModerationContext)

export const ModerationProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const [searchParams] = useSearchParams()

  const [data, setData] = useState<CreateProposalData>({
    title: "",
    duration: CREATE_PROPOSAL_DURATION_DEFAULT,
    ...(searchParams.get("type") && { type: searchParams.get("type")! as OperationType }),
    ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
    ...(searchParams.get("publicationId") && { publicationId: searchParams.get("publicationId")! }),
    ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
    ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
  })
  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)

  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const setDataOption = useCallback(
    (index: number, setState: (prevState: CreateProposalDataOption) => CreateProposalDataOption) =>
      setData(p => ({ ...p, options: p.options!.map((x, i) => (i !== index ? x : setState(x))) })),
    [],
  )

  const openMembersChangeModal = useCallback(() => setMembersChangeModalOpen(true), [])

  const value = useMemo(
    () => ({
      data,
      setData,
      setDataOption,
      isCategoriesPending,
      refetchCategories,
      categories: categories && buildCategoryTree(categories),
      openMembersChangeModal,
    }),
    [data, setDataOption, isCategoriesPending, refetchCategories, categories, openMembersChangeModal],
  )

  const handleMembersChangeModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <ModerationContext.Provider value={value}>
      {children}
      {isMembersChangeModalOpen && <MembersChangeModal onClose={handleMembersChangeModalClose} />}
    </ModerationContext.Provider>
  )
}
