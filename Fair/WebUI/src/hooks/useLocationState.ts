import { useLocation } from "react-router-dom"

/** Форма `location.state`, которую мы кладём при открытии fullscreen-страниц (см. LinkFullscreen). */
export type AppLocationState = {
  backgroundLocation?: Location
  defaultTabKey?: string
  storeId?: string
}

/** Типизированный доступ к `location.state` (никогда не возвращает null). */
export const useLocationState = (): AppLocationState => (useLocation().state as AppLocationState | null) ?? {}

/** Фоновая локация, поверх которой открыта fullscreen-страница, либо undefined. */
export const useBackgroundLocation = (): Location | undefined => useLocationState().backgroundLocation
