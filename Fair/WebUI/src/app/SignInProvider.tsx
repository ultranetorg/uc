import { createContext, useContext, PropsWithChildren, useMemo, useState, useEffect, useCallback } from "react"

import { useGetPing } from "entities/iccpNode"
import { useGetNexusUrl } from "entities/localFair"
import { useGetIccpNodeUrl } from "entities/nexus"
import { SignInModal, InstallModal } from "ui/components/specific"

export type SignInRole = "user" | "author"

type SignInContextType = {
  isPending?: boolean
  startSignIn: (role: SignInRole) => void
  openSignInModal: () => void
  openAuthorRoleRequiredModal: () => void
}

const SignInContext = createContext<SignInContextType>({
  isPending: false,
  startSignIn: () => {},
  openSignInModal: () => {},
  openAuthorRoleRequiredModal: () => {},
})

export const SignInProvider = ({ children }: PropsWithChildren) => {
  const [isIccpAvailable, setIccpAvailable] = useState(false)
  const [isSignInModalOpen, setSignInModalOpen] = useState(false)
  const [installModalRole, setInstallModalRole] = useState<SignInRole | undefined>()
  const [isAuthorRoleRequiredModalOpen, setAuthorRoleRequiredModalOpen] = useState(false)

  const nexus = useGetNexusUrl()
  const node = useGetIccpNodeUrl(nexus.data)
  const { data: pong, isPending } = useGetPing(node.data, installModalRole !== undefined ? 3000 : false)

  const handleStartSignIn = useCallback(
    (role: SignInRole) => {
      if (!isIccpAvailable) {
        setInstallModalRole(role)
      } else {
        setSignInModalOpen(true)
      }
    },
    [isIccpAvailable],
  )

  const handleInstallModalSignIn = useCallback(() => {
    setInstallModalRole(undefined)
    setSignInModalOpen(true)
  }, [])

  useEffect(() => {
    if (pong === true) setIccpAvailable(true)
  }, [pong])

  const value = useMemo<SignInContextType>(
    () => ({
      isPending,
      startSignIn: handleStartSignIn,
      openSignInModal: () => setSignInModalOpen(true),
      openAuthorRoleRequiredModal: () => setAuthorRoleRequiredModalOpen(true),
    }),
    [handleStartSignIn, isPending],
  )

  return (
    <SignInContext.Provider value={value}>
      {children}
      {isSignInModalOpen && <SignInModal onClose={() => setSignInModalOpen(false)} />}
      {installModalRole !== undefined &&
        (isIccpAvailable ? (
          <InstallModal
            variant="client-ready"
            onClose={() => setInstallModalRole(undefined)}
            onSignIn={handleInstallModalSignIn}
          />
        ) : (
          <InstallModal
            variant="client-required"
            role={installModalRole}
            onClose={() => setInstallModalRole(undefined)}
          />
        ))}
      {isAuthorRoleRequiredModalOpen && (
        <InstallModal variant="author-role-required" onClose={() => setAuthorRoleRequiredModalOpen(false)} />
      )}
    </SignInContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSignInContext = () => useContext(SignInContext)
