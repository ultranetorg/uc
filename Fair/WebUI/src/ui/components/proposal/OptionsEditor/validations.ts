import { TFunction } from "i18next"
import { UseFormClearErrors, UseFormSetError } from "react-hook-form"

import { AccountBase, AuthorBaseAvatar, CreateProposalData, CreateProposalDataOption } from "types"

export const validateUniqueCategoryTitle = (t: TFunction) => (value: unknown, data: CreateProposalData) => {
  const duplicates = data.options.filter(opt => opt.categoryTitle === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryTitle")
}

export const validateUniqueCategoryType = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options.filter(opt => opt.type === value)
  return duplicates.length <= 1 || t("validation:uniqueCategoryType")
}

export const validateUniqueFileId = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options.filter(opt => opt.fileId === value)
  return duplicates.length <= 1 || t("validation:uniqueFile")
}

export const validateUniqueParentCategory = (t: TFunction) => (value: unknown, data: CreateProposalData) => {
  if (value === undefined) {
    return t("validation:requiredCategory")
  }

  const duplicates = data.options.filter(opt => opt.parentCategoryId === value)
  if (duplicates.length > 1) {
    return t("validation:uniqueParentCategory")
  }

  const sameAsCategory = data.options.filter(opt => opt.parentCategoryId === data.categoryId)
  return sameAsCategory.length == 0 || t("validation:differentParentCategory")
}

export const validateUniqueSiteNickname = (t: TFunction) => (value: unknown, data: CreateProposalData) => {
  const duplicates = data.options.filter(opt => opt.name === value)
  return duplicates.length <= 1 || t("validation:uniqueSiteNickname")
}

export const validateUniqueTitle = (t: TFunction) => (value: string, data: CreateProposalData) => {
  const duplicates = data.options.filter(opt => opt.title === value)
  return duplicates.length <= 1 || t("validation:uniqueTitle")
}

const normalizeAuthors = (authors?: AuthorBaseAvatar[]) =>
  (authors ?? [])
    .map(x => x.id)
    .sort()
    .join("")

export const validateSiteAuthorChange = (
  t: TFunction,
  options: CreateProposalDataOption[],
  clearErrors: UseFormClearErrors<CreateProposalData>,
  setError: UseFormSetError<CreateProposalData>,
  lastEditedIndex: number,
) => {
  if (!options || lastEditedIndex >= options.length) return

  const hasDuplicates = options.some((opt, i) =>
    options.some((other, j) => i !== j && normalizeAuthors(other.authors) === normalizeAuthors(opt.authors)),
  )
  console.log(hasDuplicates)

  if (hasDuplicates) {
    setError(`options.${lastEditedIndex}`, { type: "manual", message: t("validation:uniqueOptions") })
  } else {
    clearErrors(`options.${lastEditedIndex}`)
  }
}

const normalizeAccounts = (accounts?: AccountBase[]) =>
  (accounts ?? [])
    .map(x => x.id)
    .sort()
    .join("")

export const validateSiteModeratorChange = (
  t: TFunction,
  options: CreateProposalDataOption[],
  clearErrors: UseFormClearErrors<CreateProposalData>,
  setError: UseFormSetError<CreateProposalData>,
  lastEditedIndex: number,
) => {
  if (!options || lastEditedIndex >= options.length) return

  const hasDuplicates = options.some((opt, i) =>
    options.some((other, j) => i !== j && normalizeAccounts(other.moderators) === normalizeAccounts(opt.moderators)),
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
  lastEditedIndex: number,
) => {
  if (!options || lastEditedIndex >= options.length) return

  const edited = options[lastEditedIndex]
  const hasAnyField =
    ((edited.siteTitle ?? "") as string).trim().length > 0 ||
    ((edited.slogan ?? "") as string).trim().length > 0 ||
    ((edited.description ?? "") as string).trim().length > 0

  if (!hasAnyField) {
    setError(`options.${lastEditedIndex}`, { type: "manual", message: t("validation:requiredAtLeastOneField") })
    return
  }

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
