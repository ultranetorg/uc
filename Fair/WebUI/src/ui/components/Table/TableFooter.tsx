import { ReactNode } from "react"

export type TableFooterProps = {
  colspan: number
  footer?: ReactNode
}

export const TableFooter = (props: TableFooterProps) => {
  const { colspan, footer } = props

  return (
    <tfoot>
      <tr>
        <td className="py-2" colSpan={colspan}>
          {footer}
        </td>
      </tr>
    </tfoot>
  )
}
