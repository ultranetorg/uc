import { memo } from "react"
import { Link } from "react-router-dom"
import { Trans, useTranslation } from "react-i18next"

import uosIcon from "assets/uos.png"
import { ButtonPrimary, Modal, ModalProps } from "ui/components"

type InstallFor = "author" | "user"

type InstallModalBaseProps = {
  installFor: InstallFor
  isIccpAvailable: boolean
  onSignIn: () => void
}

export type InstallModalProps = Pick<ModalProps, "onClose"> & InstallModalBaseProps

export const InstallModal = memo(({ isIccpAvailable, onSignIn, installFor, ...modalRest }: InstallModalProps) => {
  const { t } = useTranslation("installModal")

  return (
    <Modal
      className="w-135 gap-6"
      titleClassName="w-full text-center pl-8.5"
      {...modalRest}
      title={!isIccpAvailable ? t("title") : t("doneTitle")}
    >
      <div className="flex flex-col gap-6">
        {!isIccpAvailable ? (
          <>
            <Trans
              ns="installModal"
              i18nKey={installFor === "author" ? "authorText" : "userText"}
              parent="div"
              className="flex flex-col gap-3 text-center text-2sm leading-5"
              components={{ span: <span /> }}
            />
            <Link to="https://www.ultranet.org/Test/download" target="_blank">
              <ButtonPrimary label="Download" className="w-full" />
            </Link>
          </>
        ) : (
          <>
            <Trans
              ns="installModal"
              i18nKey="doneText"
              parent="div"
              className="flex flex-col gap-3 text-center text-2sm leading-5"
              components={{
                spanBold: <span className="text-base font-medium" />,
                span: <span />,
                icon: <img src={uosIcon} alt="UOS" className="mx-1 inline-block size-4 align-text-bottom" />,
              }}
            />
            <ButtonPrimary onClick={onSignIn} label="Log In" className="w-full" />
          </>
        )}
      </div>
    </Modal>
  )
})
