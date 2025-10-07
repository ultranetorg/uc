import { Input, Textarea, ValidationWrapper } from "ui/components"

import {
  ButtonMembersChange,
  DropdownSearchCategory,
  DropdownWithTranslation,
  ProductVersionSelector,
} from "./components"
import { APPROVAL_POLICIES, CATEGORY_TYPES, OPERATION_CLASSES, REVIEW_STATUSES, ROLES } from "./constants"
import { EditorField, EditorOperationFields, FieldValueType, ParameterValueType } from "./types"

export const renderByParameterValueType: Record<
  ParameterValueType,
  (field: EditorOperationFields, value: string | undefined, onChange: (value: string) => void) => JSX.Element
> = {
  category: (field, value, onChange) => (
    <DropdownSearchCategory
      key={field.parameterName}
      controlled={true}
      size="large"
      placeholder={field.parameterPlaceholder}
      value={value}
      onChange={item => onChange(item.value)}
    />
  ),
  product: (field, value, onChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  publication: (field, value, onChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  review: (field, value, onChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  user: (field, value, onChange) => (
    <Input
      key={field.parameterName}
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
}

export const renderByValueType: Record<
  FieldValueType,
  (
    field: EditorField,
    value: string | string[],
    onChange: (value: string | string[]) => void,
    errorMessage?: string,
  ) => JSX.Element
> = {
  "approval-policy": (field, _, onChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="approvalPolicies"
      items={APPROVAL_POLICIES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  "authors-array": (field, _, onDataChange) => (
    <ButtonMembersChange key={field.name} memberType="author" label={field.placeholder!} onDataChange={onDataChange} />
  ),
  category: (field, _, onChange) => (
    <DropdownSearchCategory
      key={field.name}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  "category-type": (field, _, onChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="categoryTypes"
      items={CATEGORY_TYPES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  file: field => <div key={field.name}>file</div>,
  "moderators-array": (field, _, onChange) => (
    <ButtonMembersChange key={field.name} memberType="moderator" label={field.placeholder!} onDataChange={onChange} />
  ),
  "operation-class": (field, _, onChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="operationClasses"
      items={OPERATION_CLASSES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  "review-status": (field, _, onChange) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="reviewStatuses"
      items={REVIEW_STATUSES}
      className="placeholder-gray-500"
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  roles: (field, _, onChange) => (
    <DropdownWithTranslation
      isMulti={true}
      key={field.name}
      translationKey="roles"
      className="placeholder-gray-500"
      items={ROLES}
      placeholder={field.placeholder}
      onChange={items => onChange(items.map(x => x.value).join(","))}
    />
  ),
  string: (field, value, onChange, errorMessage) => (
    <ValidationWrapper message={errorMessage}>
      <Input
        key={field.name}
        id={field.name}
        className="h-10 placeholder-gray-500"
        placeholder={field.placeholder}
        value={value as string}
        onChange={value => onChange(value)}
        error={!!errorMessage}
      />
    </ValidationWrapper>
  ),
  "string-multiline": (field, value, onChange) => (
    <Textarea
      key={field.name}
      placeholder={field.placeholder}
      className="text-2sm leading-5 placeholder-gray-500"
      value={value as string}
      onChange={value => onChange(value)}
    />
  ),
  version: (field, _, onChange) => (
    <ProductVersionSelector key={field.name} onChange={item => onChange(item.toString())} />
  ),
}
