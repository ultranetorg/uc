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
}

const SignInContext = createContext<SignInContextType>({
  isPending: false,
  startSignIn: () => {},
  openSignInModal: () => {},
})

export const SignInProvider = ({ children }: PropsWithChildren) => {
  const [isIccpAvailable, setIccpAvailable] = useState(false)
  const [isSignInModalOpen, setSignInModalOpen] = useState(false)
  const [isAuthorModalOpen, setAuthorModalOpen] = useState(false)
  const [isUserModalOpen, setUserModalOpen] = useState(false)

  const nexus = useGetNexusUrl()
  const node = useGetIccpNodeUrl(nexus.data)
  const { data: pong, isPending } = useGetPing(node.data, isAuthorModalOpen || isUserModalOpen ? 3000 : false)

  const handleSignIn = useCallback(
    (role: SignInRole) => {
      if (!isIccpAvailable) {
        if (role === "user") setUserModalOpen(true)
        else setAuthorModalOpen(true)
      } else {
        setSignInModalOpen(true)
      }
    },
    [isIccpAvailable],
  )

  const handleAuthorSignIn = useCallback(() => {
    setAuthorModalOpen(false)
    setSignInModalOpen(true)
  }, [])

  const handleUserSignIn = useCallback(() => {
    setUserModalOpen(false)
    setSignInModalOpen(true)
  }, [])

  useEffect(() => {
    if (pong === true) setIccpAvailable(true)
  }, [pong])

  const value = useMemo<SignInContextType>(
    () => ({
      isPending,
      startSignIn: handleSignIn,
      openSignInModal: () => setSignInModalOpen(true),
    }),
    [handleSignIn, isPending],
  )

  return (
    <SignInContext.Provider value={value}>
      {children}
      {isSignInModalOpen && <SignInModal onClose={() => setSignInModalOpen(false)} />}
      {isAuthorModalOpen && (
        <InstallModal
          installFor="author"
          isIccpAvailable={isIccpAvailable}
          onClose={() => setAuthorModalOpen(false)}
          onSignIn={handleAuthorSignIn}
        />
      )}
      {isUserModalOpen && (
        <InstallModal
          installFor="user"
          isIccpAvailable={isIccpAvailable}
          onClose={() => setUserModalOpen(false)}
          onSignIn={handleUserSignIn}
        />
      )}
    </SignInContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useSignInContext = () => useContext(SignInContext)
