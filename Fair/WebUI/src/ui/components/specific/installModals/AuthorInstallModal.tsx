import { memo } from "react"
import { Link } from "react-router-dom"

import { InstallModal, InstallModalProps } from "./InstallModal"

export type AuthorInstallModalProps = Omit<InstallModalProps, "children">

export const AuthorInstallModal = memo((props: AuthorInstallModalProps) => (
  <InstallModal {...props}>
    <h2>AUTHOR!!!!!!!!!!!!!!!</h2> DOWNLOAD Ultranet Client{" "}
    <Link to="https://www.ultranet.org/Test/download" target="_blank" className="text-red-500">
      Download
    </Link>
  </InstallModal>
))
