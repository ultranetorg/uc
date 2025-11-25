import { AccountBase } from "types"
import { Input, Textarea, ValidationWrapper } from "ui/components"

import {
  AddMembersPanelList,
  DropdownSearchCategory,
  DropdownWithTranslation,
  FileSelect,
  ProductVersionSelector,
  RemoveMembersPanelList,
} from "./components"
import { CATEGORY_TYPES, REVIEW_STATUSES } from "./constants"
import { EditorFieldRenderer, EditorOperationFields, FieldValueType, ParameterValueType } from "./types"

export const renderByParameterValueType: Record<
  ParameterValueType,
  (field: EditorOperationFields, value: string | undefined, onChange: (value: string) => void) => JSX.Element
> = {
  category: (field, value, onChange) => (
    <DropdownSearchCategory
      controlled={true}
      size="large"
      placeholder={field.parameterPlaceholder}
      value={value}
      onChange={item => onChange(item.value)}
    />
  ),
  product: (field, value, onChange) => (
    <Input
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  publication: (field, value, onChange) => (
    <Input
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  review: (field, value, onChange) => (
    <Input
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
  user: (field, value, onChange) => (
    <Input
      id={field.parameterName}
      placeholder={field.parameterPlaceholder}
      readOnly={true}
      value={value}
      onChange={onChange}
    />
  ),
}

export const renderByValueType: Record<FieldValueType, EditorFieldRenderer> = {
  "authors-additions": ({ errorMessage, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <AddMembersPanelList memberType="author" value={value as AccountBase[]} onChange={onChange} />
    </ValidationWrapper>
  ),
  "authors-removals": ({ errorMessage, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <RemoveMembersPanelList memberType="author" value={value as string[]} onChange={onChange} />
    </ValidationWrapper>
  ),
  category: ({ errorMessage, field, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <DropdownSearchCategory
        className="placeholder-gray-500"
        error={!!errorMessage}
        placeholder={field.placeholder}
        value={value as string}
        onChange={item => onChange(item.value)}
      />
    </ValidationWrapper>
  ),
  "category-type": ({ errorMessage, field, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <DropdownWithTranslation
        isMulti={false}
        error={!!errorMessage}
        translationKey="categoryTypes"
        items={CATEGORY_TYPES}
        placeholder={field.placeholder}
        value={value as string}
        onChange={item => onChange(item.value)}
      />
    </ValidationWrapper>
  ),
  file: ({ errorMessage, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <FileSelect value={value as string} onChange={onChange} />
    </ValidationWrapper>
  ),
  "moderators-additions": ({ errorMessage, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <AddMembersPanelList memberType="moderator" value={value as AccountBase[]} onChange={onChange} />
    </ValidationWrapper>
  ),
  "moderators-removals": ({ errorMessage, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <RemoveMembersPanelList memberType="moderator" value={value as string[]} onChange={onChange} />
    </ValidationWrapper>
  ),
  "review-status": ({ field, value, onChange }) => (
    <DropdownWithTranslation
      isMulti={false}
      translationKey="reviewStatuses"
      items={REVIEW_STATUSES}
      placeholder={field.placeholder}
      value={value as string}
      onChange={item => onChange(item.value)}
    />
  ),
  string: ({ errorMessage, field, value, onChange }) => (
    <ValidationWrapper message={errorMessage}>
      <Input
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
      placeholder={field.placeholder}
      className="text-2sm leading-5 placeholder-gray-500"
      value={value as string}
      onChange={value => onChange(value)}
    />
  ),
  version: ({ onChange }) => <ProductVersionSelector onChange={item => onChange(item.toString())} />,
}
