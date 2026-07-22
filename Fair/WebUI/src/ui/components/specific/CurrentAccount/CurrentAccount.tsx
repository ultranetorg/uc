import { useCallback, useMemo, useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useAuthenticationContext, useSignInContext, useUserContext } from "app"
import { SvgChevronRight, SvgPersonSquare } from "assets"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId, useScrollOrResize, useSubmenu } from "hooks"
import { UserAvatarChange } from "types"
import { FileUpload, FileUploadHandle, TextModal } from "ui/components"
import { fileToBase64, routes, showToast } from "utils"

import { AccountSwitcher, AccountSwitcherItem } from "./AccountSwitcher"
import { CurrentAccountButton } from "./components"
import { ProfileButton } from "./ProfileButton"
import { ProfileMenu } from "./ProfileMenu"

const STICKY_CLASSNAME = "sticky bottom-2 z-20"

export const CurrentAccount = () => {
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const { t } = useTranslation("currentAccount")
  const { mutate } = useTransactMutationWithStatus()

  const fileUploadRef = useRef<FileUploadHandle>(null)
  const profileMenu = useSubmenu({ placement: "top-start" })
  const accountsMenu = useSubmenu({ placement: "right-end" })
  useScrollOrResize(() => profileMenu.setOpen(false))

  const { user, refetch } = useUserContext()
  const { selectedUserName, users, removeUser, selectUser } = useAuthenticationContext()
  const { startSignIn, openSignInModal } = useSignInContext()

  const [deleteModalOpen, setDeleteModalOpen] = useState(false)
  const [avatarVersion, setAvatarVersion] = useState(0)

  const userItems = useMemo(
    () =>
      users.map<AccountSwitcherItem>(x => ({
        nickname: x.user.name,
        address: x.user.owner,
      })),
    [users],
  )

  const handleUpload = useCallback(
    async (file: File) => {
      const data = await fileToBase64(file)
      const operation = new UserAvatarChange(data)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:avatarUploaded"), "success")
          setAvatarVersion(v => v + 1)
          refetch()
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
      })
    },
    [mutate, refetch, t],
  )

  const handleDeleteModalConfirm = useCallback(() => {
    const operation = new UserAvatarChange(null)
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:avatarRemoved"), "success")
        setAvatarVersion(v => v + 1)
        refetch()
      },
      onError: err => {
        showToast(err.toString(), "error")
      },
      onSettled: () => setDeleteModalOpen(false),
    })
  }, [mutate, refetch, t])

  const handleDeleteModalClose = useCallback(() => setDeleteModalOpen(false), [])

  const handleAvatarChange = useCallback(() => {
    fileUploadRef.current?.show()
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, profileMenu])

  const handleAvatarDelete = useCallback(() => {
    setDeleteModalOpen(true)
    accountsMenu.setOpen(false)
    profileMenu.setOpen(false)
  }, [accountsMenu, profileMenu])

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

      if (storeId) {
        navigate(routes.store(storeId))
      }
    },
    [accountsMenu, navigate, profileMenu, selectUser, storeId],
  )

  const handleNicknameCreate = useCallback(() => alert("handleNicknameCreate"), [])

  const userSwitcherProps = useMemo(
    () => ({
      items: userItems,
      selectedUserName,
      avatarVersion,
      onAvatarChange: handleAvatarChange,
      onAvatarDelete: handleAvatarDelete,
      onAdd: handleAccountAdd,
      onRemove: handleUserRemove,
      onSelect: handleUserSelect,
    }),
    [
      userItems,
      selectedUserName,
      avatarVersion,
      handleAvatarChange,
      handleAvatarDelete,
      handleAccountAdd,
      handleUserRemove,
      handleUserSelect,
    ],
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
          avatarVersion={avatarVersion}
          ref={profileMenu.refs.setReference}
          {...profileMenu.getReferenceProps()}
        />
      )}
      {user && profileMenu.isOpen && (
        <ProfileMenu
          customParentId={profileMenu.nodeId!}
          ref={profileMenu.refs.setFloating}
          style={profileMenu.floatingStyles}
          nickname={user!.name}
          address={user!.owner!}
          hasAvatar={user!.hasAvatar}
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
      {deleteModalOpen && (
        <TextModal
          title={t("deleteAvatarModalTitle")}
          text={t("deleteAvatarModalText")}
          onClose={handleDeleteModalClose}
          onCancel={handleDeleteModalClose}
          onConfirm={handleDeleteModalConfirm}
          cancelLabel={t("common:cancel")}
          confirmLabel={t("common:delete")}
        />
      )}
      <FileUpload ref={fileUploadRef} onUpload={handleUpload} />
    </>
  )
}
