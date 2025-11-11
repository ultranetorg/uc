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
          <div className="flex h-20 w-full justify-between rounded-s border border-gray-200 p-2" onClick={handleClick}>
            <div className="h-16 w-16 overflow-hidden rounded-s">
              <img className="object-cover" src={value ? `${BASE_URL}/files/${value}` : undefined} title={value} />
            </div>
          </div>
        )}
      </div>
      {isMembersChangeModalOpen && <MemberFilesModal onClose={handleModalClose} onSelect={onChange} />}
    </>
  )
})
