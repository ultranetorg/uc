import { forwardRef, memo, useCallback, useMemo } from "react"
import { useCopyToClipboard } from "usehooks-ts"
import { useTranslation } from "react-i18next"

import { useAccountsContext } from "app"
import { PersonSquareSvg, SvgBoxArrowRight, SvgChevronRight, SvgGlobe, SvgPencilSm } from "assets"
import avatarFallback from "assets/fallback/account-avatar-11xl.png"
import { useSubmenu } from "hooks"
import { AccountBaseAvatar, PropsWithStyle } from "types"
import { CopyButton } from "ui/components/CopyButton"
import { buildAccountAvatarUrl, shortenAddress } from "utils"

import pngBackground from "./background.png"
import { AccountSwitcher } from "./AccountSwitcher"
import { MenuButton } from "./components"

type AccountMenuBaseProps = {
  accountId?: string // NOTE: Account should be passed as "accountId" not as an "id", because "id" property is already used by getFloatingProps() function of Floating UI.
  onMenuClose: () => void
  onNicknameCreate: () => void
}

export type AccountMenuProps = PropsWithStyle & Omit<AccountBaseAvatar, "id"> & AccountMenuBaseProps

export const AccountMenu = memo(
  forwardRef<HTMLDivElement, AccountMenuProps>(
    ({ style, accountId, nickname, address, onMenuClose, onNicknameCreate }, ref) => {
      const { t } = useTranslation("currentAccount")

      // const languagesMenu = useSubmenu({ placement: "right" })
      const accountMenu = useSubmenu({ placement: "right" })

      const [copiedText, copy] = useCopyToClipboard()

      const { accounts, currentAccount, authenticate, logout, selectAccount } = useAccountsContext()

      // const languageItems = useMemo(
      //   () => [
      //     {
      //       onClick: () => alert("English"),
      //       label: "English",
      //     },
      //     { onClick: () => alert("Russian"), label: "Russian" },
      //   ],
      //   [],
      // )

      const accountSwitcherItems = useMemo(() => accounts.map(x => x.account), [accounts])

      const handleCopyClick = useCallback(() => {
        copy(address)
        console.log(copiedText)
      }, [address, copiedText, copy])

      const handleAccountAdd = useCallback(() => {
        authenticate()
        onMenuClose()
      }, [authenticate, onMenuClose])

      const handleAccountSelect = useCallback(
        (index: number) => {
          selectAccount(index)
          onMenuClose()
        },
        [onMenuClose, selectAccount],
      )

      const handleLogout = () => {
        logout()
        onMenuClose()
      }

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
              {/* <MenuButton
                className="capitalize"
                label={t("common:language")}
                iconBefore={<SvgGlobe className="stroke-gray-800" />}
                iconAfter={<SvgChevronRight className="stroke-gray-800" />}
                ref={languagesMenu.refs.setReference}
                {...languagesMenu.getReferenceProps()}
              /> */}
              <MenuButton
                label={t("switchAccounts")}
                iconBefore={<PersonSquareSvg className="fill-gray-800" />}
                iconAfter={<SvgChevronRight className="stroke-gray-800" />}
                ref={accountMenu.refs.setReference}
                {...accountMenu.getReferenceProps()}
              />
              <MenuButton
                label={t("signout")}
                iconBefore={<SvgBoxArrowRight className="fill-gray-800" />}
                onClick={handleLogout}
              />
            </div>
          </div>
          {/* {languagesMenu.isOpen && (
            <SimpleMenu
              ref={languagesMenu.refs.setFloating}
              items={languageItems}
              style={languagesMenu.floatingStyles}
              onClick={() => console.log("")}
              {...languagesMenu.getFloatingProps()}
            />
          )} */}
          {accountMenu.isOpen && (
            <AccountSwitcher
              ref={accountMenu.refs.setFloating}
              style={accountMenu.floatingStyles}
              selectedItemAddress={currentAccount!.address}
              items={accountSwitcherItems}
              onAccountAdd={handleAccountAdd}
              onAccountSelect={handleAccountSelect}
              {...accountMenu.getFloatingProps()}
            />
          )}
        </>
      )
    },
  ),
)
