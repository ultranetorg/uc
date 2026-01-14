import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useManageUsersContext, useUserContext } from "app"
import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"

import { showToast } from "utils"
import { AccountSwitcher, AccountSwitcherItem } from "./AccountSwitcher"
import { CurrentAccountButton } from "./components"
import { ProfileButton } from "./ProfileButton"
import { ProfileMenu } from "./ProfileMenu"
import { SignInModal } from "./SignInModal"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const { t } = useTranslation("currentAccount")

  const profileMenu = useSubmenu({ placement: "top-start" })
  const accountsMenu = useSubmenu({ placement: "right-end" })
  useScrollOrResize(() => profileMenu.setOpen(false))

  const [showUserModal, setShowUserModal] = useState(false)

  const { user } = useUserContext()
  const {
    isPending,
    selectedUserName,
    users,
    authenticate: authenticateMutation,
    logout,
    selectUser: selectAccount,
  } = useManageUsersContext()

  const userItems = useMemo(
    () =>
      users.map<AccountSwitcherItem>(x => ({
        nickname: x.user.name,
        address: x.user.owner,
      })),
    [users],
  )

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
      selectAccount(userName)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)
    },
    [accountsMenu, profileMenu, selectAccount],
  )

  const handleNicknameCreate = useCallback(() => alert("handleNicknameCreate"), [])

  const handleSignIn = useCallback(
    (userName: string, address: string) =>
      authenticateMutation(userName, address, {
        onSuccess: data => {
          if (data === null) {
            showToast(t("authenticationCancelled"), "warning")
            return
          }

          showToast(t("successfullyAuthenticated", { userName }), "success")
          setShowUserModal(false)
        },
        onError: error => showToast(error.message, "error"),
      }),
    [authenticateMutation, t],
  )

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
          onClick={() => setShowUserModal(true)}
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
      {showUserModal && (
        <SignInModal submitDisabled={isPending} onClose={() => setShowUserModal(false)} onSubmit={handleSignIn} />
      )}
    </>
  )
}
