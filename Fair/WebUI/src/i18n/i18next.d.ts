import "i18next"

import en from "locales/en.json"
import ru from "locales/ru.json"

declare module "i18next" {
  interface CustomTypeOptions {
    defaultNS: "en"
    resources: {
      en: typeof en
      ru: typeof ru
    }
    returnNull: false
  }
}
