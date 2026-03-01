import { createContext, PropsWithChildren, useContext, useMemo, useState } from "react"
import { useLocation, useParams, useSearchParams } from "react-router-dom"
import { FormProvider, useForm } from "react-hook-form"

import { useGetCategories, useGetSitePolicies } from "entities"
import { CategoryParentBaseWithChildren, CreateProposalData, OperationType, Policy } from "types"
import { buildCategoryTree } from "utils"

type ModerationContextType = {
  lastEditedOptionIndex?: number
  setLastEditedOptionIndex: (index: number) => void
  isCategoriesPending: boolean
  refetchCategories: () => void
  categories?: CategoryParentBaseWithChildren[]
  policies?: Policy[]
}

// @ts-expect-error createContext with default value
const ModerationContext = createContext<ModerationContextType>({
  isCategoriesPending: false,
})

export const ModerationProvider = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const [searchParams] = useSearchParams()
  const location = useLocation()

  const methods = useForm<CreateProposalData>({
    mode: "onChange",
    defaultValues: {
      title: "",
      options: [],
      ...(searchParams.get("type") && { type: searchParams.get("type")! as OperationType }),
      ...(location.state?.type && { type: location.state.type as OperationType }),

      ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
      ...(searchParams.get("publicationId") && { publicationId: searchParams.get("publicationId")! }),
      ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
      ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
    },
    shouldUnregister: false,
  })

  const [lastEditedOptionIndex, setLastEditedOptionIndex] = useState<number | undefined>()

  const { data: policies } = useGetSitePolicies(siteId)
  const { data: categories, isPending: isCategoriesPending, refetch: refetchCategories } = useGetCategories(siteId)

  const value = useMemo(
    () => ({
      lastEditedOptionIndex,
      setLastEditedOptionIndex,
      isCategoriesPending,
      refetchCategories,
      categories: categories && buildCategoryTree(categories),
      policies,
    }),
    [lastEditedOptionIndex, isCategoriesPending, refetchCategories, categories, policies],
  )

  return (
    <ModerationContext.Provider value={value}>
      <FormProvider {...methods}>{children}</FormProvider>
    </ModerationContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useModerationContext = () => useContext(ModerationContext)
