import { memo } from "react"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export type FileCardProps = {
  fileId: string
  onClick: () => void
}

export const FileCard = memo(({ fileId, onClick }: FileCardProps) => (
  <div
    className="focused:border-gray-500 cursor-pointer overflow-hidden rounded border border-gray-300 bg-white text-center hover:border-gray-400"
    onClick={onClick}
    title={fileId}
  >
    <div className="size-35 overflow-hidden bg-gray-200">
      <img
        className="size-full object-cover object-center"
        src={`${BASE_URL}/files/${fileId}`}
        title={fileId}
        loading="lazy"
      />
    </div>
    <span className="truncate text-center text-2xs leading-4 text-gray-900">{fileId}</span>
  </div>
))
