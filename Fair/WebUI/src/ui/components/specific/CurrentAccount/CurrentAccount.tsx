import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useAccountsContext } from "app"
import { useScrollOrResize, useSubmenu } from "hooks"

import { AccountSwitcher } from "./AccountSwitcher"
import { CurrentAccountButton } from "./components"
import { ProfileMenu } from "./ProfileMenu"
import { ProfileButton } from "./ProfileButton"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const { t } = useTranslation("currentAccount")

  const profileMenu = useSubmenu({ placement: "top-start" })
  const accountsMenu = useSubmenu({ placement: "right-end" })
  useScrollOrResize(() => profileMenu.setOpen(false))

  const { accounts, currentAccount, authenticate, logout, selectAccount } = useAccountsContext()

  const accountItems = useMemo(() => accounts.map(x => x.account), [accounts])

  const handleAccountAdd = useCallback(() => {
    authenticate()
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, authenticate, profileMenu])

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
          onClick={() => authenticate()}
        />
      ) : !currentAccount || !currentAccount?.address ? (
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
          nickname={currentAccount?.nickname}
          id={currentAccount?.id}
          address={currentAccount?.address}
          ref={profileMenu.refs.setReference}
          {...profileMenu.getReferenceProps()}
        />
      )}
      {profileMenu.isOpen && (
        <ProfileMenu
          customParentId={profileMenu.nodeId!}
          ref={profileMenu.refs.setFloating}
          style={profileMenu.floatingStyles}
          accountId={currentAccount?.id}
          nickname={currentAccount?.nickname}
          address={currentAccount!.address!}
          onNicknameCreate={handleNicknameCreate}
          {...accountSwitcherProps}
          {...profileMenu.getFloatingProps()}
        />
      )}
      {accountsMenu.isOpen && (
        <AccountSwitcher
          ref={accountsMenu.refs.setFloating}
          style={accountsMenu.floatingStyles}
          selectedItemAddress={currentAccount?.address}
          {...accountSwitcherProps}
          {...accountsMenu.getFloatingProps()}
        />
      )}
    </>
  )
}
