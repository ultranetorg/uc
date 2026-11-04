import { useFormContext, useWatch } from "react-hook-form"

import { useGetCategoryDetails } from "entities"
import { CreateProposalData, ProductType } from "types"

/**
 * @returns Current type of the category selected in the "category-type-change" proposal form.
 */
export const useChangedCategoryType = (): ProductType | undefined => {
  const { control } = useFormContext<CreateProposalData>()
  const type = useWatch({ control, name: "type" })
  const categoryId = useWatch({ control, name: "categoryId" })

  const { data: category } = useGetCategoryDetails(type === "category-type-change" ? categoryId : undefined)

  return category?.type
}
