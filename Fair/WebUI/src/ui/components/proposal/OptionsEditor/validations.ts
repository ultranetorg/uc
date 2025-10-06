import { TFunction } from "i18next"
import { CreateProposalData } from "types"

export const validateUniqueTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.title === value)
  return duplicates.length <= 1 || t("validation:uniqueTitle")
}
