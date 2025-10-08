import { TFunction } from "i18next"
import { UseFormClearErrors, UseFormSetError } from "react-hook-form"

import { CreateProposalData, CreateProposalDataOption } from "types"

export const validateUniqueCategoryTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.categoryTitle === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryTitle")
}

export const validateUniqueCategoryType = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.type === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryType")
}

export const validateUniqueParentCategory = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.parentCategoryId === value)
  if (duplicates.length > 1) {
    return t("validation:uniqueParentCategory")
  }

  const sameAsCategory = data.options!.filter(opt => opt.parentCategoryId === data.categoryId)
  return sameAsCategory.length == 0 || t("validation:differentParentCategory")
}

export const validateUniqueSiteNickname = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.nickname === value)
  return duplicates.length <= 1 || t("validation:uniqueSiteNickname")
}

export const validateUniqueTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options!.filter(opt => opt.title === value)
  return duplicates.length <= 1 || t("validation:uniqueTitle")
}

export const validateSitePolicyChange = (
  t: TFunction,
  options: CreateProposalDataOption[],
  clearErrors: UseFormClearErrors<CreateProposalData>,
  setError: UseFormSetError<CreateProposalData>,
  lastEditedIndex?: number,
) => {
  const hasDuplicates = options.some((opt, i) =>
    options.some(
      (other, j) =>
        i !== j && other.change == opt.change && other.creators === opt.creators && other.approval === opt.approval,
    ),
  )

  if (hasDuplicates) {
    setError(`options.${lastEditedIndex}`, { type: "manual", message: t("validation:uniqueOptions") })
  } else {
    clearErrors(`options.${lastEditedIndex}`)
  }
}

export const validateSiteTextChange = (
  t: TFunction,
  options: CreateProposalDataOption[],
  clearErrors: UseFormClearErrors<CreateProposalData>,
  setError: UseFormSetError<CreateProposalData>,
  lastEditedIndex?: number,
) => {
  const hasDuplicates = options.some((opt, i) =>
    options.some(
      (other, j) =>
        i !== j &&
        ((other.siteTitle ?? "") as string).trim() == ((opt.siteTitle ?? "") as string).trim() &&
        ((other.slogan ?? "") as string).trim() === ((opt.slogan ?? "") as string).trim() &&
        ((other.description ?? "") as string).trim() === ((opt.description ?? "") as string).trim(),
    ),
  )

  if (hasDuplicates) {
    setError(`options.${lastEditedIndex}`, { type: "manual", message: t("validation:uniqueOptions") })
  } else {
    clearErrors(`options.${lastEditedIndex}`)
  }
}
