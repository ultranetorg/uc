import { memo } from "react"
import { Link } from "react-router-dom"
import { Trans, useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import uosIcon from "assets/uos.png"
import { ButtonPrimary, Modal, ModalProps } from "ui/components"

const DOWNLOAD_CLIENT_URL = "https://www.ultranet.org/net/software/download"
const HOW_TO_PUBLISH_URL = "https://www.ultranet.org/fair/docs/howtopublish"

const TEXT_CLASSNAME = "flex flex-col gap-3 text-center text-2sm leading-5"

export type InstallModalRole = "user" | "author"

export type InstallModalVariant =
  | { variant: "client-required"; role: InstallModalRole }
  | { variant: "author-role-required" }
  | { variant: "client-ready"; onSignIn: () => void }

export type InstallModalProps = Pick<ModalProps, "onClose"> & InstallModalVariant

export const InstallModal = memo((props: InstallModalProps) => {
  const { t } = useTranslation("installModal")

  const title =
    props.variant === "client-required"
      ? t("clientRequiredTitle")
      : props.variant === "author-role-required"
        ? t("authorRoleRequiredTitle")
        : t("clientReadyTitle")

  return (
    <Modal className="w-135 gap-6" titleClassName="w-full text-center pl-8.5" onClose={props.onClose} title={title}>
      <div className="flex flex-col gap-6">
        {props.variant === "client-ready" ? (
          <>
            <Trans
              ns="installModal"
              i18nKey="clientReadyText"
              parent="div"
              className={TEXT_CLASSNAME}
              components={{
                spanBold: <span className="text-base font-medium" />,
                span: <span />,
                icon: <img src={uosIcon} alt="UOS" className="mx-1 inline-block size-4 align-text-bottom" />,
              }}
            />
            <ButtonPrimary onClick={props.onSignIn} label={t("signInModal:signIn")} className="w-full" />
          </>
        ) : (
          <InstallModalGuide
            role={props.variant === "client-required" ? props.role : "author"}
            textKey={props.variant === "author-role-required" ? "authorRoleRequiredText" : undefined}
          />
        )}
      </div>
    </Modal>
  )
})

type InstallModalGuideProps = {
  role: InstallModalRole
  textKey?: string
}

const InstallModalGuide = ({ role, textKey }: InstallModalGuideProps) => {
  const { t } = useTranslation("installModal")
  const isUser = role === "user"

  return (
    <>
      <Trans
        ns="installModal"
        i18nKey={textKey ?? (isUser ? "userText" : "authorText")}
        parent="div"
        className={TEXT_CLASSNAME}
        components={{ span: <span /> }}
      />
      <Link to={isUser ? DOWNLOAD_CLIENT_URL : HOW_TO_PUBLISH_URL} target="_blank">
        <ButtonPrimary
          label={isUser ? t("common:download") : t("becomeAnAuthor")}
          className={twMerge("w-full", isUser && "capitalize")}
        />
      </Link>
    </>
  )
}
