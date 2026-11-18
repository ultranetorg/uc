import { TableColumn } from "./types"

const DEFAULT_COLUMN_MIN_WIDTH = 144
const ACTION_BUTTON_COLUMN_MIN_WIDTH = 120
const ACTION_BUTTONS_COLUMN_MIN_WIDTH = 200

const getColumnMinWidth = ({ type }: TableColumn) => {
  if (type?.endsWith("action-buttons")) return ACTION_BUTTONS_COLUMN_MIN_WIDTH
  if (type?.endsWith("action-button")) return ACTION_BUTTON_COLUMN_MIN_WIDTH
  return DEFAULT_COLUMN_MIN_WIDTH
}

export const getTableMinWidth = (columns: TableColumn[]): number =>
  columns.reduce((width, column) => width + getColumnMinWidth(column), 0)
