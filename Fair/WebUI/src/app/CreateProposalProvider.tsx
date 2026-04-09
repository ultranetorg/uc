import { createContext, PropsWithChildren, useContext, useMemo, useState } from "react"
import { useLocation, useParams, useSearchParams } from "react-router-dom"
import { FormProvider, useForm } from "react-hook-form"

import { useGetCategories } from "entities"
import { CategoryParentBaseWithChildren, CreateProposalData, OperationType } from "types"
import { buildCategoryTree } from "utils"

type CreateProposalContextType = {
  lastEditedOptionIndex?: number
  setLastEditedOptionIndex: (index: number) => void
  isCategoriesPending: boolean
  refetchCategories: () => void
  categories?: CategoryParentBaseWithChildren[]
}

// @ts-expect-error createContext with default value
const CreateProposalContext = createContext<CreateProposalContextType>({
  isCategoriesPending: false,
})

export const CreateProposalProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const [searchParams] = useSearchParams()
  const location = useLocation()

  const methods = useForm<CreateProposalData>({
    mode: "onChange",
    defaultValues: {
      title: "",
      options: [],
      ...(searchParams.get("type") && { type: searchParams.get("type")! as OperationType }),
      ...(location.state?.title && { title: location.state.title }),
      ...(location.state?.type && { type: location.state.type as OperationType }),
      ...(!!location.state.publicationId && { publicationId: location.state.publicationId }),

      ...(!!location.state.previousPath && { previousPath: location.state.previousPath }),

      ...(searchParams.get("moderatorId") && { moderatorId: searchParams.get("moderatorId")! }),
      ...(searchParams.get("publisherId") && { publisherId: searchParams.get("publisherId")! }),
      ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
      ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
      ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
    },
    shouldUnregister: false,
  })

  const [lastEditedOptionIndex, setLastEditedOptionIndex] = useState<number | undefined>()

  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const value = useMemo(
    () => ({
      lastEditedOptionIndex,
      setLastEditedOptionIndex,
      isCategoriesPending,
      refetchCategories,
      categories: categories && buildCategoryTree(categories),
    }),
    [lastEditedOptionIndex, isCategoriesPending, refetchCategories, categories],
  )

  return (
    <CreateProposalContext.Provider value={value}>
      <FormProvider {...methods}>{children}</FormProvider>
    </CreateProposalContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useCreateProposalContext = () => useContext(CreateProposalContext)
