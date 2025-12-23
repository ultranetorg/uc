import { useTranslation } from "react-i18next"

import { PROFILE_SRC } from "testConfig"
import { ButtonGhost, CopyButton } from "ui/components"
import { formatRoles } from "utils"

import { useTabsContext } from "app"
import { SvgPencilSm } from "assets"
import pngBackground from "./background.png"

export type ProfileInfoProps = {
  nickname?: string
  address: string
  roles?: string[]
  registrationDay?: number
  onTabSelect: (tab: string) => void
}

export const ProfileInfo = ({ nickname, address, roles, registrationDay, onTabSelect }: ProfileInfoProps) => {
  const { t } = useTranslation("profile")

  const { setActiveKey } = useTabsContext()

  const handleCreateNickname = () => {
    onTabSelect("profileSettings")
    setActiveKey("profileSettings")
  }

  return (
    <div className="relative flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
      <div className="bg-gray-500">
        <img src={pngBackground} alt="Background" className="size-full rounded-lg object-cover" />
      </div>
      <div className="absolute left-6 top-26.5 size-32 rounded-full bg-white p-1">
        <div className="size-30 overflow-hidden">
          <img src={PROFILE_SRC} className="size-full object-cover" />
        </div>
      </div>
      <div className="flex flex-col gap-4 px-6 pb-6 pt-21">
        <div className="flex flex-col gap-1">
          {nickname ? (
            <span className="text-xl font-semibold leading-6">{nickname}</span>
          ) : (
            <ButtonGhost
              className="text-2sm leading-5"
              label={t("createNickname")}
              iconAfter={<SvgPencilSm className="fill-gray-800" />}
              onClick={handleCreateNickname}
            />
          )}
          <div className="flex items-center gap-1">
            <span className="text-2xs leading-4 text-gray-500">{address}</span>
            <CopyButton onClick={() => console.log("Copy")} />
          </div>
        </div>
        {(roles || registrationDay) && (
          <span className="text-2sm leading-5">
            {roles && formatRoles(roles)}
            {registrationDay && (
              <>
                &nbsp;&nbsp; <span className="text-sm leading-4.25 text-gray-500">{"|"}</span> &nbsp;&nbsp;
                {registrationDay}
              </>
            )}
          </span>
        )}
      </div>
    </div>
  )
}
