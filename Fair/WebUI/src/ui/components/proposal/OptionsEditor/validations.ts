import { TFunction } from "i18next"
import { CreateProposalData } from "types"

export const validateUniqueCategoryTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.categoryTitle === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryTitle")
}

export const validateUniqueCategoryType = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.type === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryType")
}

export const validateUniqueTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.title === value)
  return duplicates.length <= 1 || t("validation:uniqueTitle")
}
