import { memo, PropsWithChildren, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Outlet, useNavigate, useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useSearchAccounts } from "entities"
import { DropdownSearchAccountsItem, DropdownSearchAccount } from "ui/components"
import { ModerationHeader } from "ui/components/specific"

export const UsersLayout = memo(({ children }: PropsWithChildren) => {
  const navigate = useNavigate()
  const { siteId, name } = useParams()
  const { t } = useTranslation("usersPage")

  const [query, setQuery] = useState("")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: searchUsers } = useSearchAccounts(debouncedQuery)

  const items = useMemo<DropdownSearchAccountsItem[]>(
    () =>
      searchUsers !== undefined
        ? searchUsers.map(x => ({
            label: x.nickname ?? x.id,
            value: x.nickname,
            avatarId: x.id,
          }))
        : [],
    [searchUsers],
  )

  const handleSelectUser = useCallback(
    (item: DropdownSearchAccountsItem) => {
      setQuery("")
      navigate(`/${siteId}/m/u/${item.value}`)
    },
    [navigate, siteId],
  )

  return (
    <>
      <ModerationHeader
        title={!name ? t("newUsersTitle") : name}
        breadcrumbTitle={!name ? t("title") : name}
        parentBreadcrumbs={
          !name
            ? { path: `/${siteId}/m`, title: t("common:proposals") }
            : [
                { path: `/${siteId}/m`, title: t("common:proposals") },
                { path: `/${siteId}/m/u`, title: t("title") },
              ]
        }
        components={
          <DropdownSearchAccount
            items={items}
            className="w-110"
            placeholder={t("placeholders:enterUserNameOrId")}
            inputValue={query}
            onInputChange={setQuery}
            onSelect={handleSelectUser}
            noOptionsLabel={t("noUsersFound")}
          />
        }
      />
      {children ?? <Outlet />}
    </>
  )
})
