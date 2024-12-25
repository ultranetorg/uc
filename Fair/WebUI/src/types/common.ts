import { TFunction } from "i18next"

export type PropsWithClassName<P = unknown> = P & {
  className?: string
}

export type PropsWithT<P = unknown> = P & {
  t: TFunction<string>
}
