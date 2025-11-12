import { SvgX } from "assets"
import { memo, useCallback, useState } from "react"

import { ButtonOutline } from "ui/components"
import { MemberFilesModal } from "ui/components/specific"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export type FileSelectProps = {
  label: string
  value?: string
  onChange: (value: string) => void
}

export const FileSelect = memo(({ label, value, onChange }: FileSelectProps) => {
  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)

  const handleClick = useCallback(() => setMembersChangeModalOpen(true), [])
  const handleModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <>
      <div className="flex flex-col gap-4">
        <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
        {value && (
          <div className="flex h-20 w-full items-center justify-between rounded border border-gray-200 p-2">
            <div className="flex items-center gap-3">
              <div className="h-16 w-16 overflow-hidden rounded">
                <img
                  className="h-full w-full object-cover object-center"
                  src={value ? `${BASE_URL}/files/${value}` : undefined}
                  title={value}
                />
              </div>
              <span className="text-2sm leading-5">{value}</span>
            </div>
            <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" />
          </div>
        )}
      </div>
      {isMembersChangeModalOpen && <MemberFilesModal onClose={handleModalClose} onSelect={onChange} />}
    </>
  )
})
