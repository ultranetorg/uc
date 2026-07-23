import { memo, useMemo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"

import { useGetUserAuthors } from "entities"
import { PropsWithClassName, UserDetails } from "types"
import { SimpleMenuItem } from "ui/components"
import { routes } from "utils"

import { HeaderDropdownButton } from "./HeaderDropdownButton"
import { MENU_ITEM_STYLE } from "./styles"

type PublisherMembersDropdownButtonBaseProps = {
  storeId: string
  t: TFunction
  user: UserDetails
}

export type PublisherMembersDropdownButtonProps = PropsWithClassName & PublisherMembersDropdownButtonBaseProps

export const PublisherMembersDropdownButton = memo(
  ({ className, storeId, t, user }: PublisherMembersDropdownButtonProps) => {
    const { data: userAuthors } = useGetUserAuthors(user.authorsIds.length > 1 ? user.id : undefined)

    const menuItems = useMemo<SimpleMenuItem[]>(
      () =>
        userAuthors?.authors.map(x => ({
          label: x.title,
          to: routes.author(x.id),
        })) ?? [],
      [userAuthors?.authors],
    )

    return user.authorsIds.length <= 1 ? (
      <Link to={routes.publisher(storeId, user!.authorsIds[0])} className={twMerge(MENU_ITEM_STYLE, "w-16")}>
        {t("common:member")}
      </Link>
    ) : (
      <HeaderDropdownButton
        className={twMerge(className, "first-letter:uppercase")}
        label={t("common:members")}
        menuItems={menuItems}
      />
    )
  },
)
