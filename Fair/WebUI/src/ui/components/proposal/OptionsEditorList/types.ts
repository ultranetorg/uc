import { OperationType } from "types"
import { ComponentBaseType } from "ui/components"

export type EditorData = {
  title: string
  data: Record<string, string | string[]>
}

export type EditorFieldType = ComponentBaseType | "select" | "select-array"

export type EditorFieldSubtype = "category" | "file" | "product" | "publication" | "review" | "user" | "version"

export type EditorField = {
  type: EditorFieldType
  subtype?: EditorFieldSubtype
  name: string
  placeholder?: string
  options?: string[]
}

export type EditorOperationFields = {
  type: OperationType
  fields: EditorField[]
}
