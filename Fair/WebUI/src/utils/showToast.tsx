import { toast } from "react-toastify"

import { Toast, ToastType } from "ui/components"

export const showToast = (text: string, type: ToastType | undefined = undefined) =>
  toast(({ closeToast }) => <Toast type={type} text={text} closeToast={closeToast} />)
