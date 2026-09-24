import { createContext, PropsWithChildren, useCallback, useContext, useMemo, useState } from "react"

import { StoreBase } from "types"

export type PendingFavoriteStoreChange = {
  store: StoreBase
  action: boolean
}

type PendingOperationsType = {
  favoritesChanges: Record<string, PendingFavoriteStoreChange>
  setFavorite: (store: StoreBase, action: boolean) => void
  clearFavorite: (storeId: string) => void
}

const PendingOperationsContext = createContext<PendingOperationsType>({
  favoritesChanges: {},
  setFavorite: () => {},
  clearFavorite: () => {},
})

export const PendingOperationsProvider = ({ children }: PropsWithChildren) => {
  const [favoritesChanges, setFavoritesChanges] = useState<Record<string, PendingFavoriteStoreChange>>({})

  const setFavorite = useCallback(
    (store: StoreBase, action: boolean) => setFavoritesChanges(prev => ({ ...prev, [store.id]: { store, action } })),
    [],
  )

  const clearFavorite = useCallback(
    (storeId: string) =>
      setFavoritesChanges(prev => {
        if (!(storeId in prev)) return prev

        const next = { ...prev }
        delete next[storeId]
        return next
      }),
    [],
  )

  const value = useMemo(
    () => ({
      favoritesChanges,
      setFavorite,
      clearFavorite,
    }),
    [favoritesChanges, setFavorite, clearFavorite],
  )

  return <PendingOperationsContext.Provider value={value}>{children}</PendingOperationsContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export const usePendingOperationsContext = () => useContext(PendingOperationsContext)
