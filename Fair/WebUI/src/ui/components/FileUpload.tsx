import { forwardRef, memo, useImperativeHandle, useRef } from "react"

export type FileUploadProps = {
  onUpload: (file: File) => void
}

export type FileUploadHandle = {
  show: () => void
}

export const FileUpload = memo(
  forwardRef<FileUploadHandle, FileUploadProps>(({ onUpload }, ref) => {
    const inputRef = useRef<HTMLInputElement>(null)

    useImperativeHandle(ref, () => ({
      show: () => inputRef.current?.click(),
    }))

    return (
      <input
        ref={inputRef}
        type="file"
        hidden
        onChange={e => {
          const file = e.target.files?.[0]
          if (file) onUpload(file)
        }}
      />
    )
  }),
)
