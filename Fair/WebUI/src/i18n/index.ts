import i18n from "i18next"
import { initReactI18next } from "react-i18next"
import LanguageDetector from "i18next-browser-languagedetector"

import en from "./locales/en"
import ru from "./locales/ru"

i18n
  // detect user language
  // learn more: https://github.com/i18next/i18next-browser-languageDetector
  .use(LanguageDetector)
  .use(initReactI18next) // passes i18n down to react-i18next
  .init({
    fallbackLng: "en",
    interpolation: {
      escapeValue: false, // react already safes from xss
    },
    keySeparator: ".",
    resources: {
      en,
      ru,
    },
    returnNull: false,
  })

export default i18n
