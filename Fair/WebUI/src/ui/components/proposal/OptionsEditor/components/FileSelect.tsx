import { memo, useCallback, useState } from "react"

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
      <div className="flex flex-col items-center justify-center">
        <div
          className="h-[117px] w-[117px] cursor-pointer overflow-hidden rounded-md bg-gray-100"
          onClick={handleClick}
        >
          <img className="object-cover" src={value ? `${BASE_URL}/files/${value}` : undefined} title={value} />
        </div>
        {!value && label}
      </div>
      {isMembersChangeModalOpen && <MemberFilesModal onClose={handleModalClose} onSelect={onChange} />}
    </>
  )
})
