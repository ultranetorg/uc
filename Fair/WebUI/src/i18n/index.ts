import i18n from "i18next"
import { initReactI18next } from "react-i18next"

import { enUS, ruRU } from "./locales"

i18n.use(initReactI18next).init({
  defaultNS: "common",
  fallbackLng: "en-US",
  interpolation: {
    escapeValue: false,
  },
  lng: "en-US",
  resources: {
    "en-US": enUS,
    "ru-RU": ruRU,
  },
  returnNull: false,
})

export default i18n
