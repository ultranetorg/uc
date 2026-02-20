import { ToastContentProps } from "react-toastify"
import { twMerge } from "tailwind-merge"

import { SvgCheckCircle, SvgExclamationOctagon, SvgExclamationTriangle, SvgInfoCircle, SvgXSm } from "assets"

export type ToastType = "default" | "success" | "warning" | "error"

type ToastBaseProps = {
  type?: ToastType
  text: string
}

export type ToastProps = Pick<ToastContentProps, "closeToast"> & ToastBaseProps

export const Toast = ({ type = "default", text, closeToast }: ToastProps) => (
  <div
    className={twMerge(
      "flex w-full max-w-115 items-start gap-4 rounded-lg bg-gray-800 p-4 text-white shadow-[0_13px_18px_0_rgba(0,0,0,0.1)]",
      type === "warning" && "bg-[#F88545]",
      type === "error" && "bg-[#CD3841]",
    )}
  >
    <div className="flex w-full items-start gap-2">
      {type === "success" && <SvgCheckCircle className="shrink-0 stroke-white" />}
      {type === "warning" && <SvgExclamationTriangle className="shrink-0 fill-white" />}
      {type === "error" && <SvgExclamationOctagon className="shrink-0 fill-white" />}
      {type === "default" && <SvgInfoCircle className="shrink-0 stroke-white" />}

      <span className="mt-1 w-full break-words text-2xs font-medium leading-4">{text}</span>
    </div>

    <button
      className="flex size-6 shrink-0 cursor-pointer items-center justify-center rounded-md hover:bg-white/10"
      onClick={closeToast}
    >
      <SvgXSm className="fill-[#EEF1F8] hover:fill-white" />
    </button>
  </div>
)
