import { useCallback } from "react"
import { To, useNavigate } from "react-router-dom"

import { useBackgroundLocation } from "./useLocationState"

/**
 * Закрытие fullscreen-страницы: если она открыта поверх фоновой локации — возвращаемся на неё,
 * иначе уходим на `fallback` (по умолчанию — на шаг назад по истории).
 */
export const useCloseFullscreen = (fallback?: To) => {
  const navigate = useNavigate()
  const backgroundLocation = useBackgroundLocation()

  return useCallback(() => {
    if (backgroundLocation) navigate(backgroundLocation)
    else if (fallback !== undefined) navigate(fallback)
    else navigate(-1)
  }, [navigate, backgroundLocation, fallback])
}
