import { memo, useRef, useState } from "react"

import { TFunction } from "i18next"

import { ButtonPrimary } from "ui/components"

export type UploadZoneProps = {
  t: TFunction
  showEmptyState?: boolean
  onUploadClick: () => void
}

export const UploadZone = memo(({ t, showEmptyState = true, onUploadClick }: UploadZoneProps) => {
  const inputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)

  const handleFile = (file: File) => {
    //uploadFile(file).then(console.log)
    console.log(file)
  }

  return (
    <>
      <div
        className="flex h-full flex-col items-center justify-center gap-4 text-2sm leading-5"
        onDragOver={e => {
          e.preventDefault()
          setDragging(true)
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={e => {
          e.preventDefault()
          setDragging(false)
          const file = e.dataTransfer.files[0]
          if (file) handleFile(file)
        }}
      >
        {showEmptyState && <span>{t("noImages")}</span>}
        <ButtonPrimary className="capitalize" label={t("common:upload")} onClick={() => inputRef.current?.click()} />
        <span>{t("dragImage")}</span>

        <input
          ref={inputRef}
          type="file"
          hidden
          onChange={e => {
            const file = e.target.files?.[0]
            if (file) handleFile(file)
          }}
        />
      </div>
    </>
  )
})
