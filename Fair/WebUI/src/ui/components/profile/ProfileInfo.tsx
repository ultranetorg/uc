import { PROFILE_SRC } from "testConfig"
import { formatRoles } from "utils"

import pngBackground from "./background.png"
import { SvgFilesSm } from "assets"

export type ProfileInfoProps = {
  nickname?: string
  address: string
  roles: string[]
  registrationDay: number
}

export const ProfileInfo = ({ nickname, address, roles, registrationDay }: ProfileInfoProps) => {
  return (
    <div className="relative flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
      <div className="bg-[#2A2A2A]">
        <img src={pngBackground} alt="Background" className="h-full w-full rounded-lg object-cover" />
      </div>
      <div className="top-26.5 absolute left-6 h-32 w-32 rounded-full bg-white px-1 py-1">
        <div className="h-30 w-30 overflow-hidden">
          <img src={PROFILE_SRC} className="h-full w-full object-cover" />
        </div>
      </div>
      <div className="flex flex-col gap-4 px-6 pb-6 pt-21">
        <div className="flex flex-col gap-1">
          <span className="text-xl font-semibold leading-6">{nickname}</span>
          <div className="flex items-center gap-1">
            <span className="text-2xs leading-4 text-gray-500">{address}</span>
            <SvgFilesSm className="fill-gray-500" />
          </div>
        </div>
        <span className="text-2sm leading-5">
          {formatRoles(roles)}
          &nbsp;&nbsp; <span className="text-sm leading-4.25 text-gray-500">{"|"}</span> &nbsp;&nbsp;
          {registrationDay}
        </span>
      </div>
    </div>
  )
}
