import { Input, Textarea, ValidationWrapper } from "ui/components"

import {
  ButtonMembersChange,
  DropdownSearchCategory,
  DropdownWithTranslation,
  ProductVersionSelector,
} from "./components"
import { APPROVAL_POLICIES, CATEGORY_TYPES, OPERATION_CLASSES, REVIEW_STATUSES, ROLES } from "./constants"
import { EditorFieldRenderer, EditorOperationFields, FieldValueType, ParameterValueType } from "./types"

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

export const renderByValueType: Record<FieldValueType, EditorFieldRenderer> = {
  "approval-policy": ({ field, onChange }) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="approvalPolicies"
      items={APPROVAL_POLICIES}
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  "authors-additions": ({ errorMessage, field, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <ButtonMembersChange
        key={field.name}
        changeAction="add"
        memberType="author"
        label={field.placeholder!}
        onChange={onChange}
      />
    </ValidationWrapper>
  ),
  "authors-removals": ({ errorMessage, field, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <ButtonMembersChange
        key={field.name}
        changeAction="remove"
        memberType="author"
        label={field.placeholder!}
        value={value}
        onChange={onChange}
      />
    </ValidationWrapper>
  ),
  category: ({ errorMessage, field, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <DropdownSearchCategory
        key={field.name}
        className="placeholder-gray-500"
        error={!!errorMessage}
        placeholder={field.placeholder}
        value={value}
        onChange={item => onChange(item.value)}
      />
    </ValidationWrapper>
  ),
  "category-type": ({ errorMessage, field, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <DropdownWithTranslation
        isMulti={false}
        key={field.name}
        error={!!errorMessage}
        translationKey="categoryTypes"
        items={CATEGORY_TYPES}
        placeholder={field.placeholder}
        onChange={item => onChange(item.value)}
      />
    </ValidationWrapper>
  ),
  file: ({ field }) => <div key={field.name}>file</div>,
  "moderators-additions": ({ field, value, onChange }) => (
    <ButtonMembersChange
      key={field.name}
      changeAction="add"
      memberType="moderator"
      label={field.placeholder!}
      value={value}
      onChange={onChange}
    />
  ),
  "moderators-removals": ({ field, value, onChange }) => (
    <ButtonMembersChange
      key={field.name}
      changeAction="remove"
      memberType="moderator"
      label={field.placeholder!}
      value={value}
      onChange={onChange}
    />
  ),
  "operation-class": ({ field, onChange }) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="operationClasses"
      items={OPERATION_CLASSES}
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  "review-status": ({ field, onChange }) => (
    <DropdownWithTranslation
      isMulti={false}
      key={field.name}
      translationKey="reviewStatuses"
      items={REVIEW_STATUSES}
      placeholder={field.placeholder}
      onChange={item => onChange(item.value)}
    />
  ),
  roles: ({ field, onChange }) => (
    <DropdownWithTranslation
      isMulti={true}
      key={field.name}
      translationKey="roles"
      items={ROLES}
      placeholder={field.placeholder}
      onChange={items =>
        onChange(
          items
            .map(x => x.value)
            .sort()
            .join(","),
        )
      }
    />
  ),
  string: ({ errorMessage, field, value, onChange }) => (
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
  "string-multiline": ({ field, value, onChange }) => (
    <Textarea
      key={field.name}
      placeholder={field.placeholder}
      className="text-2sm leading-5 placeholder-gray-500"
      value={value as string}
      onChange={value => onChange(value)}
    />
  ),
  version: ({ field, onChange }) => (
    <ProductVersionSelector key={field.name} onChange={item => onChange(item.toString())} />
  ),
}
