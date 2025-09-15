import { TFunction } from "i18next"

import { EditorOperationFields } from "./types"

export const getEditorOperationsFields = (t: TFunction): EditorOperationFields[] => [
  {
    type: "category-avatar-change",
    fields: [
      {
        type: "select",
        subtype: "category",
        name: "categoryId",
        placeholder: t("placeholders:selectCategory"),
      },
      {
        type: "select",
        subtype: "file",
        name: "fileId",
        placeholder: t("placeholders:selectFile"),
      },
    ],
  },
  {
    type: "category-creation",
    fields: [
      {
        type: "select",
        subtype: "category",
        name: "parentCategoryId",
        placeholder: t("placeholders:selectParentCategory"),
      },
      {
        type: "input",
        name: "categoryTitle",
        placeholder: t("placeholders:categoryTitle"),
      },
    ],
  },
  {
    type: "category-deletion",
    fields: [
      {
        type: "select",
        subtype: "category",
        name: "categoryId",
        placeholder: t("placeholders:selectCategory"),
      },
    ],
  },
  {
    type: "category-movement",
    fields: [
      {
        type: "select",
        subtype: "category",
        name: "categoryId",
        placeholder: t("placeholders:selectCategory"),
      },
      {
        type: "select",
        subtype: "category",
        name: "parentCategoryId",
        placeholder: t("placeholders:selectParentCategory"),
      },
    ],
  },
  {
    type: "category-type-change",
    fields: [
      {
        type: "select",
        subtype: "category",
        name: "categoryId",
        placeholder: t("placeholders:selectCategory"),
      },
      {
        type: "dropdown",
        name: "type",
        placeholder: t("placeholders:selectCategoryType"),
      },
    ],
  },
  {
    type: "publication-creation",
    fields: [
      {
        type: "dropdown",
        options: ["a", "b", "c", "d"],
        name: "dropdownId",
        placeholder: t("placeholders:selectCategory"),
      },
      {
        type: "input",
        name: "inputId",
        placeholder: t("placeholders:selectCategoryType"),
      },
      {
        type: "number",
        name: "numberId",
        placeholder: t("placeholders:selectCategoryType"),
      },
      {
        type: "textarea",
        name: "textareaId",
        placeholder: t("placeholders:selectCategoryType"),
      },
      {
        type: "select-array",
        name: "selectArrayId",
        placeholder: t("placeholders:selectCategoryType"),
      },
    ],
  },
]
