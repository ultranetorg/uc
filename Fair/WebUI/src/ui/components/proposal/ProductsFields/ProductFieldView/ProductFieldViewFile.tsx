import { memo } from "react"
import { ProductFieldViewProp } from "./types"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

function Img({ src, alt, className = "" }: { src?: string; alt?: string; className?: string }) {
  return <img alt={alt ?? "image"} className={`h-full w-full object-cover ${className}`} src={src} />
}

function PreviewBox({ children, label }: { children: React.ReactNode; label?: string }) {
  return (
    <div className="flex flex-col items-center text-sm">
      <div className="relative h-28 w-40 overflow-hidden rounded-md border bg-gray-50">{children}</div>
      {label ? <div className="mt-1 text-xs text-gray-500">{label}</div> : null}
    </div>
  )
}

export const ProductFieldViewFile = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
  const newSrc = `${BASE_URL}/files/${value}`
  const oldSrc = `${BASE_URL}/files/${oldValue}`

  switch (status) {
    case "added":
      return (
        <PreviewBox label="Added">
          <div className="h-full w-full p-0.5">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-green-500">
              <Img src={newSrc} alt="added image" />
            </div>
          </div>
        </PreviewBox>
      )
    case "removed":
      return (
        <PreviewBox label="Removed">
          <div className="relative h-full w-full">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-red-500">
              <Img src={oldSrc} alt="removed image" className="opacity-60" />
            </div>
            <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
              <div className="rounded bg-red-600/30 px-2 py-1 text-sm text-white">Removed</div>
            </div>
          </div>
        </PreviewBox>
      )
    case "changed":
      return (
        <div className="flex items-start gap-3">
          <PreviewBox label="Old">
            <div className="relative h-full w-full">
              <div className="h-full w-full overflow-hidden rounded-md border-2 border-red-500">
                <Img src={oldSrc} alt="old image" className="opacity-70" />
              </div>
            </div>
          </PreviewBox>

          <PreviewBox label="New">
            <div className="h-full w-full">
              <div className="h-full w-full overflow-hidden rounded-md border-2 border-green-500">
                <Img src={newSrc} alt="new image" />
              </div>
            </div>
          </PreviewBox>
        </div>
      )
    default:
      return (
        <div className="h-full w-full">
          <div className="h-full w-full overflow-hidden rounded-md">
            <Img src={newSrc} alt="image" />
          </div>
        </div>
      )
  }
})
