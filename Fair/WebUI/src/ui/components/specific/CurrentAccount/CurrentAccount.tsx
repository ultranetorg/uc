import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { useAuthenticationContext, useSignInContext, useUserContext } from "app"
import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"

import { AccountSwitcher, AccountSwitcherItem } from "./AccountSwitcher"
import { CurrentAccountButton } from "./components"
import { ProfileButton } from "./ProfileButton"
import { ProfileMenu } from "./ProfileMenu"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const navigate = useNavigate()
  const { siteId } = useParams()
  const { t } = useTranslation("currentAccount")

  const profileMenu = useSubmenu({ placement: "top-start" })
  const accountsMenu = useSubmenu({ placement: "right-end" })
  useScrollOrResize(() => profileMenu.setOpen(false))

  const { user } = useUserContext()
  const { selectedUserName, users, removeUser, selectUser } = useAuthenticationContext()
  const { startSignIn, openSignInModal } = useSignInContext()

  const userItems = useMemo(
    () =>
      users.map<AccountSwitcherItem>(x => ({
        nickname: x.user.name,
        address: x.user.owner,
      })),
    [users],
  )

  const handleAuthenticate = useCallback(() => startSignIn("user"), [startSignIn])

  const handleAccountAdd = useCallback(() => {
    openSignInModal()
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, openSignInModal, profileMenu])

  const handleUserRemove = useCallback(
    (userName: string) => {
      removeUser(userName)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)
    },
    [accountsMenu, removeUser, profileMenu],
  )

  const handleUserSelect = useCallback(
    (userName: string) => {
      selectUser(userName)
      accountsMenu.setOpen(false)
      profileMenu.setOpen(false)

      if (siteId) {
        navigate(`/${siteId}`)
      }
    },
    [accountsMenu, navigate, profileMenu, selectUser, siteId],
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
          label={t("switchUsers")}
          ref={accountsMenu.refs.setReference}
          {...accountsMenu.getReferenceProps()}
        />
      ) : (
        <CurrentAccountButton
          className={STICKY_CLASSNAME}
          nickname={user.name}
          id={user.id}
          address={user.owner}
          ref={profileMenu.refs.setReference}
          {...profileMenu.getReferenceProps()}
        />
      )}
      {profileMenu.isOpen && (
        <ProfileMenu
          customParentId={profileMenu.nodeId!}
          ref={profileMenu.refs.setFloating}
          style={profileMenu.floatingStyles}
          nickname={user!.name}
          address={user!.owner!}
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
    </>
  )
}
