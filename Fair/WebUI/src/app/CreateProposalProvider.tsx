import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"
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

  const buildDefaultValues = () => {
    const prePopulatedOptions = [
      ...(location.state?.moderators ? [{ moderators: location.state.moderators }] : []),
      ...(location.state?.authors ? [{ authors: location.state.authors }] : []),
    ]
    return {
      title: "",
      options: prePopulatedOptions.length > 0 ? prePopulatedOptions : [{ title: "" }],
      ...(location.state?.title && { title: location.state.title }),
      ...(location.state?.type && { type: location.state.type as OperationType }),
      ...(location.state?.publicationId && { publicationId: location.state.publicationId }),
      ...(location.state?.categoryId && { categoryId: location.state.categoryId }),
      ...(!!location.state?.previousPath && { previousPath: location.state.previousPath }),
      ...(searchParams.get("publisherId") && { publisherId: searchParams.get("publisherId")! }),
      ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
      ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
      ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
    }
  }

  const methods = useForm<CreateProposalData>({
    mode: "onChange",
    defaultValues: buildDefaultValues(),
    shouldUnregister: false,
  })

  useEffect(() => {
    methods.reset(buildDefaultValues())
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.state?.type])

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
