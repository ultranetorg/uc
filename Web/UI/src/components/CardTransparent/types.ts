import { ReactNode } from "react"

export type CardTransparentRow = {
  accessor: string
  label?: string
  type?: string
  fullRow?: boolean
}

export type CardTransparentValueRenderer = (value: any, row: CardTransparentRow) => ReactNode
