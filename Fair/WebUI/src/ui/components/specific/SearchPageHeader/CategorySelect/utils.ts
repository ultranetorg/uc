import { CategoryParentBaseWithChildren } from "types"

// Selecting a category always selects its whole subtree and deselecting always clears its ancestors, so a selected id
// implies its descendants are selected too. Only the topmost ids are sent — the backend expands an id to its descendants.
export const collectTopmostSelectedIds = (
  categories: CategoryParentBaseWithChildren[],
  selectedIds: ReadonlySet<string>,
): string[] =>
  categories.flatMap(category =>
    selectedIds.has(category.id) ? category.id : collectTopmostSelectedIds(category.children, selectedIds),
  )
