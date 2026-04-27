import { ReactNode } from "react"
import { Trans } from "react-i18next"
import { Link } from "react-router-dom"

import {
  CategoryAvatarChange,
  CategoryCreation,
  CategoryDeletion,
  CategoryMovement,
  CategoryTypeChange,
  ProposalOption,
  PublicationPublish,
  SiteAuthorsRemoval,
  SiteAvatarChange,
  SiteModeratorAddition,
  SiteModeratorRemoval,
  SiteNameChange,
  SiteTextChange,
} from "types"
import { AccountsList } from "ui/components"
import { buildFileUrl } from "utils"

const getCategoryAvatarChange = (siteId: string, operation: CategoryAvatarChange): JSX.Element => (
  <>
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        CategoryLink: (
          <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
    />
    <img src={buildFileUrl(operation.fileId)} loading="lazy" className="size-35 rounded" />
  </>
)

const getCategoryCreation = (siteId: string, operation: CategoryCreation): JSX.Element =>
  operation.parentCategoryId ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        ParentLink: (
          <Link to={`/${siteId}/c/${operation.parentCategoryId}`} className="underline">
            {operation.parentCategoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
      values={{ categoryTitle: operation.title }}
    />
  ) : (
    <Trans
      ns="proposalView"
      i18nKey={`${operation.$type}_root`}
      components={{
        ParentLink: (
          <Link to={`/${siteId}/c/${operation.parentCategoryId}`} className="underline">
            {operation.parentCategoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
      values={{ categoryTitle: operation.title }}
    />
  )

const getCategoryDeletion = (siteId: string, operation: CategoryDeletion): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      CategoryLink: (
        <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
  />
)

const getCategoryMovement = (siteId: string, operation: CategoryMovement): JSX.Element =>
  operation.parentCategoryId ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        CategoryLink: (
          <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
        ParentLink: (
          <Link to={`/${siteId}/c/${operation.parentCategoryId}`} className="underline">
            {operation.parentCategoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
    />
  ) : (
    <Trans
      ns="proposalView"
      i18nKey={`${operation.$type}_root`}
      components={{
        CategoryLink: (
          <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
    />
  )

const getCategoryTypeChange = (siteId: string, operation: CategoryTypeChange): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      CategoryLink: (
        <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
    values={{ categoryType: operation.categoryType, type: operation.type }}
  />
)

const getPublicationPublish = (siteId: string, operation: PublicationPublish): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      PublicationLink: (
        <Link to={`/${siteId}/m/c/u/${operation.publicationId}`} className="underline">
          {operation.publicationTitle}
        </Link>
      ),
      CategoryLink: (
        <Link to={`/${siteId}/c/${operation.categoryId}`} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
  />
)

const getSiteAvatarChange = (operation: SiteAvatarChange): JSX.Element => (
  <>
    <Trans ns="proposalView" i18nKey={operation.$type} parent={"p"} />
    <img src={buildFileUrl(operation.fileId)} loading="lazy" className="size-35 rounded" />
  </>
)

const getSiteNameChange = (operation: SiteNameChange): JSX.Element =>
  operation.siteName ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      values={{ name: operation.name, siteName: operation.siteName }}
      parent={"p"}
    />
  ) : (
    <Trans
      ns="proposalView"
      i18nKey={`${operation.$type}_empty`}
      values={{ siteName: operation.siteName }}
      parent={"p"}
    />
  )

const getSiteTextChange = (operation: SiteTextChange): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    values={{ title: operation.title, description: operation.description, slogan: operation.slogan }}
    parent={"p"}
  />
)

const getSiteAuthorsRemoval = (operation: SiteAuthorsRemoval): JSX.Element => {
  return <b>{operation.authorId}</b>
}

const getSiteModeratorAddition = (operation: SiteModeratorAddition): JSX.Element => {
  return (
    <div className="flex flex-col gap-2">
      <Trans
        ns="proposalView"
        i18nKey={`${operation.$type}`}
        parent={"p"}
        className="text-2sm leading-5"
        count={operation.candidates.length}
      />
      <AccountsList items={operation.candidates.map(x => ({ id: x.id, title: x.name, avatarId: x.id }))} />
    </div>
  )
}

const getSiteModeratorRemoval = (operation: SiteModeratorRemoval): JSX.Element => {
  return (
    <div className="flex flex-col gap-2">
      <Trans ns="proposalView" i18nKey={`${operation.$type}`} parent={"p"} className="text-2sm leading-5" />
      <AccountsList
        items={[{ id: operation.moderator.id, title: operation.moderator.name, avatarId: operation.moderator.id }]}
      />
    </div>
  )
}

export const renderDescription = (siteId: string, option: ProposalOption): ReactNode => {
  switch (option.operation.$type) {
    case "category-avatar-change":
      return getCategoryAvatarChange(siteId, option.operation as CategoryAvatarChange)
    case "category-creation":
      return getCategoryCreation(siteId, option.operation as CategoryCreation)
    case "category-deletion":
      return getCategoryDeletion(siteId, option.operation as CategoryDeletion)
    case "category-movement":
      return getCategoryMovement(siteId, option.operation as CategoryMovement)
    case "category-type-change":
      return getCategoryTypeChange(siteId, option.operation as CategoryTypeChange)

    case "publication-publish":
      return getPublicationPublish(siteId, option.operation as PublicationPublish)

    case "site-avatar-change":
      return getSiteAvatarChange(option.operation as SiteAvatarChange)
    case "site-name-change":
      return getSiteNameChange(option.operation as SiteNameChange)
    case "site-text-change":
      return getSiteTextChange(option.operation as SiteTextChange)

    case "site-authors-removal":
      return getSiteAuthorsRemoval(option.operation as SiteAuthorsRemoval)
    case "site-moderator-addition":
      return getSiteModeratorAddition(option.operation as SiteModeratorAddition)
    case "site-moderator-removal":
      return getSiteModeratorRemoval(option.operation as SiteModeratorRemoval)
  }

  return null
}
