import { TFunction } from "i18next"
import { ReactNode } from "react"
import {
  CategoryAvatarChange,
  CategoryCreation,
  CategoryDeletion,
  CategoryMovement,
  CategoryTypeChange,
  ProposalOption,
  SiteAuthorsChange,
  SiteAvatarChange,
  SiteModeratorAddition,
  SiteModeratorRemoval,
  SiteNicknameChange,
  SiteTextChange,
} from "types"

export const getOptionTitle = (t: TFunction, option: ProposalOption) => {
  return t("operations:" + option.operation.$type)
}

const getCategoryAvatarChange = (operation: CategoryAvatarChange): JSX.Element => {
  return (
    <b>
      {operation.categoryId} + " " + {operation.fileId}
    </b>
  )
}

const getCategoryCreation = (operation: CategoryCreation): JSX.Element => {
  return (
    <b>
      {operation.title} + " " + {operation.parentCategoryId}
    </b>
  )
}

const getCategoryDeletion = (operation: CategoryDeletion): JSX.Element => {
  return <b>{operation.categoryId}</b>
}

const getCategoryMovement = (operation: CategoryMovement): JSX.Element => {
  return (
    <b>
      {operation.categoryId} + " " + {operation.parentCategoryId}
    </b>
  )
}

const getCategoryTypeChange = (operation: CategoryTypeChange): JSX.Element => {
  return (
    <b>
      {operation.categoryId} + " " + {operation.type}
    </b>
  )
}

const getSiteAuthorsChange = (operation: SiteAuthorsChange): JSX.Element => {
  return (
    <b>
      {operation.additionsIds.join(", ")} + " " + {operation.removalsIds.join(", ")}
    </b>
  )
}

const getSiteAvatarChange = (operation: SiteAvatarChange): JSX.Element => {
  return <b>{operation.fileId}</b>
}

const getSiteModeratorAddition = (operation: SiteModeratorAddition): JSX.Element => {
  return <b>{operation.candidatesIds.join(", ")}</b>
}

const getSiteModeratorRemoval = (operation: SiteModeratorRemoval): JSX.Element => {
  return <b>{operation.moderatorId.join(", ")}</b>
}

const getSiteNameChange = (operation: SiteNicknameChange): JSX.Element => {
  return <b>{operation.name}</b>
}

const getSiteTextChange = (operation: SiteTextChange): JSX.Element => {
  return (
    <b>
      {operation.title} + " " + {operation.slogan} + " " + {operation.description}
    </b>
  )
}

export const getOptionDescription = (t: TFunction, option: ProposalOption): ReactNode => {
  switch (option.operation.$type) {
    case "category-avatar-change":
      return getCategoryAvatarChange(option.operation as CategoryAvatarChange)
    case "category-creation":
      return getCategoryCreation(option.operation as CategoryCreation)
    case "category-deletion":
      return getCategoryDeletion(option.operation as CategoryDeletion)
    case "category-movement":
      return getCategoryMovement(option.operation as CategoryMovement)
    case "category-type-change":
      return getCategoryTypeChange(option.operation as CategoryTypeChange)
    case "site-authors-change":
      return getSiteAuthorsChange(option.operation as SiteAuthorsChange)
    case "site-avatar-change":
      return getSiteAvatarChange(option.operation as SiteAvatarChange)
    case "site-moderator-addition":
      return getSiteModeratorAddition(option.operation as SiteModeratorAddition)
    case "site-moderator-removal":
      return getSiteModeratorRemoval(option.operation as SiteModeratorRemoval)
    case "site-name-change":
      return getSiteNameChange(option.operation as SiteNicknameChange)
    case "site-text-change":
      return getSiteTextChange(option.operation as SiteTextChange)
  }

  return null
}
