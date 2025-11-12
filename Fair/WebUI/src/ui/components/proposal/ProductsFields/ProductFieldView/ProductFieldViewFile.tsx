import { memo } from "react"
import { ProductFieldViewProp } from "./types"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

function Img({ src, alt, className = "" }: { src?: string; alt?: string; className?: string }) {
  return <img alt={alt ?? "image"} className={`w-full h-full object-cover ${className}`} src={src} />
}

function PreviewBox({ children, label }: { children: React.ReactNode; label?: string }) {
  return (
    <div className="flex flex-col items-center text-sm">
      <div className="w-40 h-28 overflow-hidden rounded-md border bg-gray-50 relative">{children}</div>
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
          <div className="w-full h-full p-0.5">
            <div className="w-full h-full border-2 border-green-500 rounded-md overflow-hidden">
              <Img src={newSrc} alt="added image" />
            </div>
          </div>
        </PreviewBox>
      )
    case "removed":
      return (
        <PreviewBox label="Removed">
          <div className="w-full h-full relative">
            <div className="w-full h-full border-2 border-red-500 rounded-md overflow-hidden">
              <Img src={oldSrc} alt="removed image" className="opacity-60" />
            </div>
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              <div className="bg-red-600/30 text-white text-sm px-2 py-1 rounded">Removed</div>
            </div>
          </div>
        </PreviewBox>
      )
    case "changed":
      return (
        <div className="flex gap-3 items-start">
          <PreviewBox label="Old">
            <div className="w-full h-full relative">
              <div className="w-full h-full border-2 border-red-500 rounded-md overflow-hidden">
                <Img src={oldSrc} alt="old image" className="opacity-70" />
              </div>
            </div>
          </PreviewBox>

          <PreviewBox label="New">
            <div className="w-full h-full">
              <div className="w-full h-full border-2 border-green-500 rounded-md overflow-hidden">
                <Img src={newSrc} alt="new image" />
              </div>
            </div>
          </PreviewBox>
        </div>
      )
    default:
      return (
        <div className="w-full h-full">
          <div className="w-full h-full overflow-hidden rounded-md">
            <Img src={newSrc} alt="image" />
          </div>
        </div>
      )
  }
})
