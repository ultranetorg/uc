import { twMerge } from "tailwind-merge"
import { useLocalStorage } from "usehooks-ts"
import { Trans } from "react-i18next"

import { SvgXSm } from "assets"
import { LINKS, LOCAL_STORAGE_KEYS } from "constants/"
import { PropsWithClassName } from "types"

export const PageBanner = ({ className }: PropsWithClassName) => {
  const [showBanner, setShowBanner] = useLocalStorage(LOCAL_STORAGE_KEYS.SHOW_WEB3_BANNER, true)

  const handleClose = () => setShowBanner(false)

  if (!showBanner) {
    return null
  }

  return (
    <div className={twMerge("flex items-center justify-center bg-black", className)}>
      <Trans
        ns="pageBanner"
        i18nKey="text"
        parent="div"
        className="mx-auto my-0 py-4 text-center text-2xs leading-4 text-white"
        components={{
          1: <a href={LINKS.ULTRANET} className="text-[#B1D5F5]" target="_blank" />,
          2: <a href={LINKS.ULTRANET_DOWNLOAD} className="text-[#B1D5F5]" target="_blank" />,
        }}
      />
      <SvgXSm className="mr-3 cursor-pointer fill-gray-400 hover:fill-white" onClick={handleClose} />
    </div>
  )
}
