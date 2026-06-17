import { memo } from "react"
import { Link } from "react-router-dom"

import { InstallModal, InstallModalProps } from "./InstallModal"

export type UserInstallModalProps = Omit<InstallModalProps, "children">

export const UserInstallModal = memo((props: UserInstallModalProps) => (
  <InstallModal {...props}>
    <h2>USER!!!!!!!!!!!!!!!</h2> DOWNLOAD Ultranet Client{" "}
    <Link to="https://www.ultranet.org/Test/download" target="_blank" className="text-red-500">
      Download
    </Link>
  </InstallModal>
))
