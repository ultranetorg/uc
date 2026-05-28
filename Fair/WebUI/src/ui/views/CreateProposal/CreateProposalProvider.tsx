import { createContext, PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"
import { useLocation } from "react-router-dom"
import { FormProvider, useForm } from "react-hook-form"

import { CreateProposalData, OperationType } from "types"

type CreateProposalContextType = {
  lastEditedOptionIndex?: number
  setLastEditedOptionIndex: (index: number) => void
}

const CreateProposalContext = createContext<CreateProposalContextType>({
  setLastEditedOptionIndex: () => {},
})

export const CreateProposalProvider = ({ children }: PropsWithChildren) => {
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
      ...(location.state?.categoryId !== undefined && { categoryId: location.state.categoryId }),
      ...(location.state?.userId && { userId: location.state?.userId }),
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

  const value = useMemo(
    () => ({
      lastEditedOptionIndex,
      setLastEditedOptionIndex,
    }),
    [lastEditedOptionIndex],
  )

  return (
    <CreateProposalContext.Provider value={value}>
      <FormProvider {...methods}>{children}</FormProvider>
    </CreateProposalContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useCreateProposalContext = () => useContext(CreateProposalContext)
