import { forwardRef, memo, useCallback } from "react"
import { useCopyToClipboard } from "usehooks-ts"
import { useTranslation } from "react-i18next"

import { useAccountsContext } from "app"
import { SvgPersonSquare, SvgChevronRight, SvgPencilSm } from "assets"
import avatarFallback from "assets/fallback/account-avatar-11xl.png"
import { useSubmenu } from "hooks"
import { AccountBaseAvatar, PropsWithStyle } from "types"
import { CopyButton } from "ui/components/CopyButton"
import { buildAccountAvatarUrl, shortenAddress } from "utils"

import pngBackground from "./background.png"
import { AccountSwitcher, AccountSwitcherItem } from "./AccountSwitcher"
import { ProfileButton } from "./ProfileButton"

type ProfileMenuBaseProps = {
  customParentId: string
  accountId?: string // NOTE: Account should be passed as "accountId" not as an "id", because "id" property is already used by getFloatingProps() function of Floating UI.
  items: AccountSwitcherItem[]
  onAdd: () => void
  onRemove: (index: number) => void
  onSelect: (index: number) => void
  onNicknameCreate: () => void
}

export type ProfileMenuProps = PropsWithStyle & Omit<AccountBaseAvatar, "id"> & ProfileMenuBaseProps

export const ProfileMenu = memo(
  forwardRef<HTMLDivElement, ProfileMenuProps>(
    (
      { customParentId, style, accountId, nickname, address, items, onAdd, onRemove, onSelect, onNicknameCreate },
      ref,
    ) => {
      const { t } = useTranslation("currentAccount")

      const accountMenu = useSubmenu({ placement: "right-end", customParentId })

      const [copiedText, copy] = useCopyToClipboard()

      const { currentAccount } = useAccountsContext()

      const handleCopyClick = useCallback(() => {
        copy(address)
        console.log(copiedText)
      }, [address, copiedText, copy])

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
                title={nickname ?? address}
              >
                <img
                  className="size-full object-cover object-center"
                  src={accountId != null ? buildAccountAvatarUrl(accountId) : avatarFallback}
                  loading="lazy"
                  onError={e => {
                    e.currentTarget.onerror = null
                    e.currentTarget.src = avatarFallback
                  }}
                />
              </div>
            </div>
            <div className="flex flex-col gap-2 px-6 py-2">
              {nickname ? (
                <span
                  className="overflow-hidden text-ellipsis text-nowrap text-xl font-semibold leading-6 text-gray-800"
                  title={address}
                >
                  {nickname}
                </span>
              ) : (
                <div
                  className="flex cursor-pointer items-center gap-1"
                  title={t("createNickname")}
                  onClick={onNicknameCreate}
                >
                  <span className="text-2sm leading-5">{t("createNickname")}</span>
                  <SvgPencilSm className="text-gray-800" />
                </div>
              )}
              <div className="flex items-center gap-1">
                <span
                  className="overflow-hidden text-ellipsis text-nowrap text-2xs leading-3.5 text-gray-500"
                  title={address}
                >
                  {shortenAddress(address)}
                </span>
                <CopyButton onClick={handleCopyClick} />
              </div>
            </div>
            <div className="flex flex-col gap-2 p-6">
              {/*
              //TODO: should be uncommented later.
              <Link to={`/p/abc`} state={{ backgroundLocation: location }}>
                <MenuButton label="Profile" />
              </Link>
            */}
              <ProfileButton
                label={t("switchAccounts")}
                iconBefore={<SvgPersonSquare className="fill-gray-800" />}
                iconAfter={<SvgChevronRight className="stroke-gray-800" />}
                ref={accountMenu.refs.setReference}
                {...accountMenu.getReferenceProps()}
              />
            </div>
          </div>
          {accountMenu.isOpen && (
            <AccountSwitcher
              ref={accountMenu.refs.setFloating}
              style={accountMenu.floatingStyles}
              selectedItemAddress={currentAccount!.address}
              items={items}
              onAdd={onAdd}
              onRemove={onRemove}
              onSelect={onSelect}
              {...accountMenu.getFloatingProps()}
            />
          )}
        </>
      )
    },
  ),
)
