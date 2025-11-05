import { memo } from "react"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export type FileCardProps = {
  fileId: string
  onClick: () => void
}

export const FileCard = memo(({ fileId, onClick }: FileCardProps) => (
  <div
    className="h-[117px] w-[117px] cursor-pointer overflow-hidden rounded-md border-transparent hover:border"
    onClick={onClick}
  >
    <img className="object-cover" src={`${BASE_URL}/files/${fileId}`} title={fileId} />
  </div>
))
