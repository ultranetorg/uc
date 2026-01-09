import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useManageUsersContext, useUserContext } from "app"
import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"

import { showToast } from "utils"
import { AccountSwitcher } from "./AccountSwitcher"
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
  const { accounts, isPending, authenticateMutation, logout, selectAccount } = useManageUsersContext()

  const accountItems = useMemo(() => accounts.map(x => x.account), [accounts])

  const handleAccountAdd = useCallback(() => {
    setShowUserModal(true)
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, profileMenu])

  const handleAccountRemove = useCallback(
    (index: number) => {
      logout(index)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)
    },
    [accountsMenu, logout, profileMenu],
  )

  const handleAccountSelect = useCallback(
    (index: number) => {
      selectAccount(index)
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

  const accountSwitcherProps = useMemo(
    () => ({
      items: accountItems,
      onAdd: handleAccountAdd,
      onRemove: handleAccountRemove,
      onSelect: handleAccountSelect,
    }),
    [accountItems, handleAccountAdd, handleAccountRemove, handleAccountSelect],
  )

  return (
    <>
      {!accounts.length ? (
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
          accountId={user!.id}
          address={user!.address!}
          onNicknameCreate={handleNicknameCreate}
          {...accountSwitcherProps}
          {...profileMenu.getFloatingProps()}
        />
      )}
      {accountsMenu.isOpen && (
        <AccountSwitcher
          ref={accountsMenu.refs.setFloating}
          style={accountsMenu.floatingStyles}
          selectedItemAddress={user!.address}
          {...accountSwitcherProps}
          {...accountsMenu.getFloatingProps()}
        />
      )}
      {showUserModal && (
        <SignInModal
          submitDisabled={isPending}
          submitLabel={t("common:signIn")}
          title={t("common:signIn")}
          onClose={() => setShowUserModal(false)}
          onSubmit={handleSignIn}
        />
      )}
    </>
  )
}
