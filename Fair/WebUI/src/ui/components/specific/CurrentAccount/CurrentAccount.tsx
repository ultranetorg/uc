import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useManageUsersContext, useUserContext } from "app"
import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"
import { SignInModal } from "ui/components/specific"

import { AccountSwitcher, AccountSwitcherItem } from "./AccountSwitcher"
import { CurrentAccountButton } from "./components"
import { ProfileButton } from "./ProfileButton"
import { ProfileMenu } from "./ProfileMenu"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const { t } = useTranslation("currentAccount")

  const profileMenu = useSubmenu({ placement: "top-start" })
  const accountsMenu = useSubmenu({ placement: "right-end" })
  useScrollOrResize(() => profileMenu.setOpen(false))

  const [showSignInModal, setShowUserModal] = useState(false)

  const { user } = useUserContext()
  const { selectedUserName, users, logout, selectUser } = useManageUsersContext()

  const userItems = useMemo(
    () =>
      users.map<AccountSwitcherItem>(x => ({
        nickname: x.user.name,
        address: x.user.owner,
      })),
    [users],
  )

  const handleAuthenticate = useCallback(() => setShowUserModal(true), [])

  const handleAccountAdd = useCallback(() => {
    setShowUserModal(true)
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, profileMenu])

  const handleUserRemove = useCallback(
    (userName: string) => {
      logout(userName)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)
    },
    [accountsMenu, logout, profileMenu],
  )

  const handleUserSelect = useCallback(
    (userName: string) => {
      selectUser(userName)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)
    },
    [accountsMenu, profileMenu, selectUser],
  )

  const handleNicknameCreate = useCallback(() => alert("handleNicknameCreate"), [])

  const userSwitcherProps = useMemo(
    () => ({
      items: userItems,
      selectedUserName,
      onAdd: handleAccountAdd,
      onRemove: handleUserRemove,
      onSelect: handleUserSelect,
    }),
    [userItems, handleAccountAdd, handleUserRemove, handleUserSelect, selectedUserName],
  )

  return (
    <>
      {!users.length ? (
        <ProfileButton
          iconBefore={<SvgPersonSquare className="fill-gray-800" />}
          className={STICKY_CLASSNAME}
          label={t("authenticate")}
          onClick={handleAuthenticate}
        />
      ) : !user ? (
        <ProfileButton
          iconBefore={<SvgPersonSquare className="fill-gray-800" />}
          iconAfter={<SvgChevronRight className="stroke-gray-800" />}
          className={STICKY_CLASSNAME}
          label={t("switchAccounts")}
          ref={accountsMenu.refs.setReference}
          {...accountsMenu.getReferenceProps()}
        />
      ) : (
        <CurrentAccountButton
          className={STICKY_CLASSNAME}
          nickname={user.nickname}
          id={user.id}
          address={user.address}
          ref={profileMenu.refs.setReference}
          {...profileMenu.getReferenceProps()}
        />
      )}
      {profileMenu.isOpen && (
        <ProfileMenu
          customParentId={profileMenu.nodeId!}
          ref={profileMenu.refs.setFloating}
          style={profileMenu.floatingStyles}
          nickname={user!.nickname}
          address={user!.address!}
          onNicknameCreate={handleNicknameCreate}
          {...userSwitcherProps}
          {...profileMenu.getFloatingProps()}
        />
      )}
      {accountsMenu.isOpen && (
        <AccountSwitcher
          ref={accountsMenu.refs.setFloating}
          style={accountsMenu.floatingStyles}
          {...userSwitcherProps}
          {...accountsMenu.getFloatingProps()}
        />
      )}
      {showSignInModal && <SignInModal onClose={() => setShowUserModal(false)} />}
    </>
  )
}
