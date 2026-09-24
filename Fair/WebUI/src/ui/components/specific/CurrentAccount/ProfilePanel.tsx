import { forwardRef, memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgChevronRight, SvgPerson2, SvgPersonSquare, SvgX } from "assets"
import avatarFallback from "assets/fallback/user-22.5.png"
import { useSubmenu } from "hooks"
import { UserBaseAvatar, PropsWithStyle } from "types"
import { ImageFallback } from "ui/components"
import { buildUserAvatarByNameUrl } from "utils"

import { AccountMenu, AccountMenuBaseProps } from "./AccountMenu/AccountMenu"
import { ProfileButton } from "./ProfileButton"
import { AvatarMenu } from "./AvatarMenu/AvatarMenu"

type ProfilePanelBaseProps = {
  customParentId: string
  hasAvatar: boolean
  onNicknameCreate: () => void
  onAvatarChange: () => void
  onAvatarDelete: () => void
  onClose: () => void
} & AccountMenuBaseProps

export type ProfilePanelProps = PropsWithStyle & Omit<UserBaseAvatar, "id"> & ProfilePanelBaseProps

export const ProfilePanel = memo(
  forwardRef<HTMLDivElement, ProfilePanelProps>(
    ({ customParentId, hasAvatar, style, selectedUserName, address, onClose, ...userSwitcherProps }, ref) => {
      const { t } = useTranslation("currentAccount")

      const avatarMenu = useSubmenu({ placement: "right-start", customParentId, offset: 2 })
      const accountMenu = useSubmenu({ placement: "right-start", customParentId, offset: 2 })

      return (
        <>
          <div
            className="z-10 flex w-[340px] flex-col overflow-hidden rounded-lg bg-gray-800 text-white shadow-md"
            ref={ref}
            style={style}
          >
            <div className="flex w-full justify-end p-3">
              <SvgX className="cursor-pointer stroke-gray-300 hover:stroke-white" onClick={onClose} />
            </div>
            <div className="flex w-full flex-col items-center gap-4 p-2">
              <div className="size-[90px] overflow-hidden rounded" title={selectedUserName}>
                <ImageFallback
                  src={`${buildUserAvatarByNameUrl(selectedUserName!)}?v=${userSwitcherProps.avatarVersion ?? 0}`}
                  fallbackSrc={avatarFallback}
                />
              </div>
              <span
                className="min-w-0 overflow-hidden text-ellipsis text-nowrap text-xl font-semibold leading-6"
                title={address}
              >
                {selectedUserName}
              </span>
            </div>
            <div className="flex flex-col gap-2 p-2">
              {!hasAvatar ? (
                <ProfileButton
                  label={t("setAvatar")}
                  iconBefore={<SvgPersonSquare className="fill-white" />}
                  onClick={userSwitcherProps.onAvatarChange}
                />
              ) : (
                <ProfileButton
                  label={t("changeAvatar")}
                  iconBefore={<SvgPersonSquare className="fill-white" />}
                  iconAfter={<SvgChevronRight className="stroke-white" />}
                  ref={avatarMenu.refs.setReference}
                  {...avatarMenu.getReferenceProps()}
                />
              )}
              <ProfileButton
                label={t("switchUsers")}
                iconBefore={<SvgPerson2 className="fill-white" />}
                iconAfter={<SvgChevronRight className="stroke-white" />}
                ref={accountMenu.refs.setReference}
                {...accountMenu.getReferenceProps()}
              />
            </div>
          </div>
          {avatarMenu.isOpen && (
            <AvatarMenu
              ref={avatarMenu.refs.setFloating}
              style={avatarMenu.floatingStyles}
              t={t}
              onChange={userSwitcherProps.onAvatarChange}
              onDelete={userSwitcherProps.onAvatarDelete}
              {...userSwitcherProps}
              {...avatarMenu.getFloatingProps()}
            />
          )}
          {accountMenu.isOpen && (
            <AccountMenu
              ref={accountMenu.refs.setFloating}
              style={accountMenu.floatingStyles}
              selectedUserName={selectedUserName}
              {...userSwitcherProps}
              {...accountMenu.getFloatingProps()}
            />
          )}
        </>
      )
    },
  ),
)
