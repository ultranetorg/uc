import { memo, PropsWithChildren, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Outlet, useNavigate, useParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"
import { capitalize } from "lodash"

import { SEARCH_DELAY } from "config"
import { useSearchAccounts } from "entities"
import { DropdownSearchAccountsItem, DropdownSearchAccount } from "ui/components"
import { ModerationHeader } from "ui/components/specific"

export const UsersLayout = memo(({ children }: PropsWithChildren) => {
  const navigate = useNavigate()
  const { siteId, tabKey, userId } = useParams()
  const { t } = useTranslation("usersPage")

  const [query, setQuery] = useState("")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)

  const { data: searchUsers } = useSearchAccounts(debouncedQuery)

  const items = useMemo<DropdownSearchAccountsItem[]>(
    () =>
      searchUsers !== undefined
        ? searchUsers.map(x => ({
            value: x.id,
            label: x.nickname,
            avatarId: x.id,
          }))
        : [],
    [searchUsers],
  )

  const headerTitle = useMemo(() => {
    if (tabKey === "n") return t("newUsers")
    else if (tabKey === "r") return t("userRemovals")
    return capitalize(t("common:users"))
  }, [t, tabKey])

  const handleSelectUser = useCallback(
    (item: DropdownSearchAccountsItem) => {
      setQuery("")
      navigate(`/${siteId}/m/u/u/${item.value}`)
    },
    [navigate, siteId],
  )

  return (
    <>
      <ModerationHeader
        title={headerTitle}
        breadcrumbTitle={!userId ? t("title") : userId}
        parentBreadcrumbs={userId !== undefined ? { path: `/${siteId}/m/u/u`, title: t("common:users") } : undefined}
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
