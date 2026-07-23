import { ReactNode } from "react"
import { Trans } from "react-i18next"
import { Link } from "react-router-dom"

import authorFallback from "assets/fallback/author-8.png"
import userFallback from "assets/fallback/user-16.png"
import {
  CategoryAvatarChange,
  CategoryCreation,
  CategoryDeletion,
  CategoryMovement,
  CategoryTypeChange,
  ProposalOption,
  PublicationPublish,
  PublicationUnpublish,
  StoreAuthorsRemoval,
  StoreAvatarChange,
  StoreModeratorAddition,
  StoreModeratorRemoval,
  StoreNameChange,
  StoreInfoUpdation,
} from "types"
import { AccountsList } from "ui/components"
import { MembersList } from "ui/components/MembersList"
import { buildFileUrl, buildUserAvatarByIdUrl, routes } from "utils"

const getCategoryAvatarChange = (storeId: string, operation: CategoryAvatarChange): JSX.Element => (
  <>
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        CategoryLink: (
          <Link to={routes.category(storeId, operation.categoryId)} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
    />
    <img src={buildFileUrl(operation.fileId)} loading="lazy" className="size-35 rounded" />
  </>
)

const getCategoryCreation = (storeId: string, operation: CategoryCreation): JSX.Element =>
  operation.parentCategoryId ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        ParentLink: (
          <Link to={routes.category(storeId, operation.parentCategoryId)} className="underline">
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
          <Link to={routes.category(storeId, operation.parentCategoryId ?? "")} className="underline">
            {operation.parentCategoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
      values={{ categoryTitle: operation.title }}
    />
  )

const getCategoryDeletion = (storeId: string, operation: CategoryDeletion): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      CategoryLink: (
        <Link to={routes.category(storeId, operation.categoryId)} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
  />
)

const getCategoryMovement = (storeId: string, operation: CategoryMovement): JSX.Element =>
  operation.parentCategoryId ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      components={{
        CategoryLink: (
          <Link to={routes.category(storeId, operation.categoryId)} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
        ParentLink: (
          <Link to={routes.category(storeId, operation.parentCategoryId)} className="underline">
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
          <Link to={routes.category(storeId, operation.categoryId)} className="underline">
            {operation.categoryTitle}
          </Link>
        ),
      }}
      parent={"p"}
    />
  )

const getCategoryTypeChange = (storeId: string, operation: CategoryTypeChange): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      CategoryLink: (
        <Link to={routes.category(storeId, operation.categoryId)} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
    values={{ categoryType: operation.categoryType, type: operation.type }}
  />
)

const getPublicationPublish = (storeId: string, operation: PublicationPublish): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      PublicationLink: (
        <Link to={routes.moderation.unpublishedPublication(storeId, operation.publicationId)} className="underline">
          {operation.publicationTitle}
        </Link>
      ),
      CategoryLink: (
        <Link to={routes.category(storeId, operation.categoryId)} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
  />
)

const getPublicationUnpublish = (storeId: string, operation: PublicationPublish): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    components={{
      PublicationLink: (
        <Link to={routes.publication(storeId, operation.publicationId)} className="underline">
          {operation.publicationTitle}
        </Link>
      ),
      CategoryLink: (
        <Link to={routes.category(storeId, operation.categoryId)} className="underline">
          {operation.categoryTitle}
        </Link>
      ),
    }}
    parent={"p"}
  />
)

const getStoreAvatarChange = (operation: StoreAvatarChange): JSX.Element => (
  <>
    <Trans ns="proposalView" i18nKey={operation.$type} parent={"p"} />
    <img src={buildFileUrl(operation.fileId)} loading="lazy" className="size-35 rounded" />
  </>
)

const getStoreNameChange = (operation: StoreNameChange): JSX.Element =>
  operation.storeName ? (
    <Trans
      ns="proposalView"
      i18nKey={operation.$type}
      values={{ name: operation.name, storeName: operation.storeName }}
      parent={"p"}
    />
  ) : (
    <Trans ns="proposalView" i18nKey={`${operation.$type}_empty`} values={{ name: operation.name }} parent={"p"} />
  )

const getStoreInfoUpdation = (operation: StoreInfoUpdation): JSX.Element => (
  <Trans
    ns="proposalView"
    i18nKey={`${operation.$type}`}
    values={{ title: operation.title, description: operation.description, slogan: operation.slogan }}
    parent={"p"}
  />
)

const getStoreAuthorsRemoval = (operation: StoreAuthorsRemoval): JSX.Element => {
  return (
    <div className="flex flex-col gap-2">
      <Trans
        ns="proposalView"
        i18nKey={`${operation.$type}`}
        parent={"p"}
        className="text-2sm leading-5"
        count={operation.removals.length}
      />
      <MembersList
        items={operation.removals.map(x => ({ id: x.id, title: x.title, avatarSrc: buildFileUrl(x.avatarId) }))}
        fallbackSrc={authorFallback}
      />
    </div>
  )
}

const getStoreModeratorAddition = (operation: StoreModeratorAddition): JSX.Element => {
  return (
    <div className="flex flex-col gap-2">
      <Trans
        ns="proposalView"
        i18nKey={`${operation.$type}`}
        parent={"p"}
        className="text-2sm leading-5"
        count={operation.candidates.length}
      />
      <AccountsList
        items={operation.candidates.map(x => ({ id: x.id, title: x.name, avatarId: x.id }))}
        fallbackSrc={userFallback}
      />
    </div>
  )
}

const getStoreModeratorRemoval = (operation: StoreModeratorRemoval): JSX.Element => {
  return (
    <div className="flex flex-col gap-2">
      <Trans ns="proposalView" i18nKey={`${operation.$type}`} parent={"p"} className="text-2sm leading-5" />
      <AccountsList
        items={[
          {
            id: operation.moderator.id,
            title: operation.moderator.name,
            avatarSrc: buildUserAvatarByIdUrl(operation.moderator.id),
          },
        ]}
        fallbackSrc={userFallback}
      />
    </div>
  )
}

export const renderDescription = (storeId: string, option: ProposalOption): ReactNode => {
  switch (option.operation.$type) {
    case "category-avatar-change":
      return getCategoryAvatarChange(storeId, option.operation as CategoryAvatarChange)
    case "category-creation":
      return getCategoryCreation(storeId, option.operation as CategoryCreation)
    case "category-deletion":
      return getCategoryDeletion(storeId, option.operation as CategoryDeletion)
    case "category-movement":
      return getCategoryMovement(storeId, option.operation as CategoryMovement)
    case "category-type-change":
      return getCategoryTypeChange(storeId, option.operation as CategoryTypeChange)

    case "publication-publish":
      return getPublicationPublish(storeId, option.operation as PublicationPublish)
    case "publication-unpublish":
      return getPublicationUnpublish(storeId, option.operation as PublicationUnpublish)

    case "store-avatar-change":
      return getStoreAvatarChange(option.operation as StoreAvatarChange)
    case "store-name-change":
      return getStoreNameChange(option.operation as StoreNameChange)
    case "store-info-updation":
      return getStoreInfoUpdation(option.operation as StoreInfoUpdation)

    case "store-authors-removal":
      return getStoreAuthorsRemoval(option.operation as StoreAuthorsRemoval)
    case "store-moderator-addition":
      return getStoreModeratorAddition(option.operation as StoreModeratorAddition)
    case "store-moderator-removal":
      return getStoreModeratorRemoval(option.operation as StoreModeratorRemoval)
  }

  return null
}
