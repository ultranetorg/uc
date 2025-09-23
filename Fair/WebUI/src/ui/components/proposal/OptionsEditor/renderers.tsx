import { Dropdown, Input, Textarea } from "ui/components"

import { DropdownSearchCategory, DropdownWithTranslation, ProductVersionSelector } from "./components"
import { APPROVAL_POLICIES, CATEGORY_TYPES, OPERATION_CLASSES, REVIEW_STATUSES, ROLES } from "./constants"
import { EditorField, EditorOperationFields, FieldValueType, ParameterValueType } from "./types"

const DEBUG_USER_ITEMS = [
  { label: "User 1", value: "debug-user-1" },
  { label: "User 2", value: "debug-user-2" },
  { label: "User 3", value: "debug-user-3" },
]

export const renderByParameterValueType: Record<
  ParameterValueType,
  (
    field: EditorOperationFields,
    value: string | undefined,
    onDataChange: (name: string, value: string) => void,
  ) => JSX.Element
> = {
  category: (field, value, onDataChange) => (
    <DropdownSearchCategory
      key={field.parameterName}
      controlled={true}
      size="large"
      placeholder={field.parameterPlaceholder}
      value={value}
      onChange={item => onDataChange(field.parameterName!, item.value)}
    />
  ),
  product: (field, value, onDataChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={value => onDataChange(field.parameterName!, value)}
    />
  ),
  publication: (field, value, onDataChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={value => onDataChange(field.parameterName!, value)}
    />
  ),
  review: (field, value, onDataChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={value => onDataChange(field.parameterName!, value)}
    />
  ),
  user: (field, _, onDataChange) => (
    <Dropdown
      isMulti={false}
      key={field.parameterName}
      items={DEBUG_USER_ITEMS}
      size="large"
      placeholder={field.parameterPlaceholder}
      onChange={item => onDataChange(field.parameterName!, item.value)}
    />
  ),
}

export const renderByValueType: Record<
  FieldValueType,
  (
    field: EditorField,
    value: string | string[],
    onDataChange: (name: string, value: string | string[]) => void,
  ) => JSX.Element
> = {
  "approval-policy": (field, _, onDataChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="approvalPolicies"
      items={APPROVAL_POLICIES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onDataChange(field.name, item.value)}
    />
  ),
  category: (field, _, onDataChange) => (
    <DropdownSearchCategory
      key={field.name}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onDataChange(field.name, item.value)}
    />
  ),
  "category-type": (field, _, onDataChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="categoryTypes"
      items={CATEGORY_TYPES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onDataChange(field.name, item.value)}
    />
  ),
  file: field => <div key={field.name}>file</div>,
  "operation-class": (field, _, onDataChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="operationClasses"
      items={OPERATION_CLASSES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onDataChange(field.name, item.value)}
    />
  ),
  "review-status": (field, _, onDataChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="reviewStatuses"
      items={REVIEW_STATUSES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onDataChange(field.name, item.value)}
    />
  ),
  roles: (field, _, onDataChange) => (
    <DropdownWithTranslation
      isMulti={true}
      key={field.name}
      translationKey="roles"
      className="placeholder-gray-500"
      items={ROLES}
      placeholder={field.placeholder}
      onChange={items => onDataChange(field.name, items.map(x => x.value).join(","))}
    />
  ),
  string: (field, value, onDataChange) => (
    <Input
      key={field.name}
      id={field.name}
      className="h-10 placeholder-gray-500"
      placeholder={field.placeholder}
      value={value as string}
      onChange={value => onDataChange(field.name, value)}
    />
  ),
  "string-multiline": (field, value, onDataChange) => (
    <Textarea
      key={field.name}
      placeholder={field.placeholder}
      className="text-2sm leading-5 placeholder-gray-500"
      value={value as string}
      onChange={value => onDataChange(field.name, value)}
    />
  ),
  "user-array": field => <div key={field.name}>user-array</div>,
  version: (field, _, onDataChange) => (
    <ProductVersionSelector key={field.name} onChange={item => onDataChange(field.name!, item.toString())} />
  ),
}
