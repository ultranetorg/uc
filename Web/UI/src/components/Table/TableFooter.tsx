import { ReactNode } from "react"

export type TableFooterProps = {
  colspan: number
  footer?: ReactNode
}

export const TableFooter = (props: TableFooterProps) => {
  const { colspan, footer } = props

  return (
    <tfoot className="h-[52px]">
      <tr>
        <td className="px-4" colSpan={colspan}>
          {footer}
        </td>
      </tr>
    </tfoot>
  )
}
