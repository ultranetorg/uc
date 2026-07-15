import { forwardRef, memo } from "react"
import { useTranslation } from "react-i18next"

import { SvgChevronRight, SvgPencilSm, SvgPerson2, SvgPersonSquare } from "assets"
import avatarFallback from "assets/fallback/user-22.5.png"
import { useSubmenu } from "hooks"
import { AccountBaseAvatar, PropsWithStyle } from "types"
import { ButtonGhost, CopyAddressButton, ImageFallback } from "ui/components"
import { buildUserAvatarByNameUrl } from "utils"

import pngBackground from "./background.png"
import { AccountSwitcher, AccountSwitcherBaseProps } from "./AccountSwitcher"
import { ProfileButton } from "./ProfileButton"
import { AvatarMenu } from "./AvatarMenu"

type ProfileMenuBaseProps = {
  customParentId: string
  hasAvatar: boolean
  onNicknameCreate: () => void
  onAvatarChange: () => void
  onAvatarDelete: () => void
} & AccountSwitcherBaseProps

export type ProfileMenuProps = PropsWithStyle & Omit<AccountBaseAvatar, "id"> & ProfileMenuBaseProps

export const ProfileMenu = memo(
  forwardRef<HTMLDivElement, ProfileMenuProps>(
    ({ customParentId, hasAvatar, style, selectedUserName, address, ...userSwitcherProps }, ref) => {
      const { t } = useTranslation("currentAccount")

      const avatarMenu = useSubmenu({ placement: "right-end", customParentId })
      const accountMenu = useSubmenu({ placement: "right-end", customParentId })

      return (
        <>
          <div
            // TODO: should be uncommented later h-[392px]
            className="z-10 w-[340px] overflow-hidden rounded-lg border border-gray-300 bg-gray-75 shadow-[0_4px_14px_0_rgba(28,38,58,0.1)]"
            ref={ref}
            style={style}
          >
            <div className="relative h-[169px]">
              <div className="h-[120px] w-[340px] bg-[#2A2A2A]">
                <img src={pngBackground} alt="Background" className="size-full rounded-lg object-cover" />
              </div>
              <div className="absolute bottom-0 left-[20px] size-[98px] rounded-full bg-gray-75" />
              <div
                className="absolute bottom-[4px] left-[24px] size-[90px] overflow-hidden rounded-full"
                title={selectedUserName}
              >
                <ImageFallback
                  src={`${buildUserAvatarByNameUrl(selectedUserName!)}?v=${userSwitcherProps.avatarVersion ?? 0}`}
                  fallbackSrc={avatarFallback}
                />
              </div>
            </div>
            <div className="flex flex-col gap-2 px-6 py-2">
              {selectedUserName ? (
                <span
                  className="overflow-hidden text-ellipsis text-nowrap text-xl font-semibold leading-6 text-gray-800"
                  title={address}
                >
                  {selectedUserName}
                </span>
              ) : (
                /*<LinkFullscreen to={routes.profile(address)} params={{ defaultTabKey: "profileSettings" }}>*/
                <ButtonGhost
                  className="text-2sm leading-5"
                  label={t("createNickname")}
                  iconAfter={<SvgPencilSm className="fill-gray-800" />}
                  onClick={() => console.log("Create nickname")}
                />
                /*</LinkFullscreen>*/
              )}
              <CopyAddressButton address={address} />
            </div>
            <div className="flex flex-col gap-2 p-6">
              {/* <LinkFullscreen to={`/p/${address}`}>
                <ProfileButton iconBefore={<SvgPersonSquare className="fill-gray-800" />} label={t("profile")} />
              </LinkFullscreen> */}
              {!hasAvatar ? (
                <ProfileButton
                  label={t("setAvatar")}
                  iconBefore={<SvgPersonSquare className="fill-gray-800" />}
                  onClick={userSwitcherProps.onAvatarChange}
                />
              ) : (
                <ProfileButton
                  label={t("changeAvatar")}
                  iconBefore={<SvgPersonSquare className="fill-gray-800" />}
                  iconAfter={<SvgChevronRight className="stroke-gray-800" />}
                  ref={avatarMenu.refs.setReference}
                  {...avatarMenu.getReferenceProps()}
                />
              )}

              <ProfileButton
                label={t("switchUsers")}
                iconBefore={<SvgPerson2 className="fill-gray-800" />}
                iconAfter={<SvgChevronRight className="stroke-gray-800" />}
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
            <AccountSwitcher
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
