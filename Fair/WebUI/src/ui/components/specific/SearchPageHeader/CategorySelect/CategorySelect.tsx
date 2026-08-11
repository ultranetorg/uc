import { forwardRef, memo, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"
import {
  autoUpdate,
  FloatingPortal,
  offset,
  safePolygon,
  useClick,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

import { useStoreContext } from "app"
import { SvgDropdownIndicator } from "assets"
import { CategoryParentBaseWithChildren } from "types"
import { ButtonPrimary, ResetAllButton } from "ui/components"

import { CategorySelectItem } from "./CategorySelectItem"
import { collectTopmostSelectedIds } from "./utils"

// How many categories a user may pick at once — counted the same way the label counts them, i.e. over the topmost ids.
export const MAX_SELECTED_CATEGORIES = 6

export type CategorySelectProps = {
  placeholder: string
  // Seeds the selection once the category tree is loaded — later changes are ignored, the select owns its state afterwards.
  initialCategoryIds?: string[]
  onChange: (categoryIds: string[] | undefined) => void
}

export type CategorySelectHandle = {
  reset: () => void
}

export const CategorySelect = memo(
  forwardRef<CategorySelectHandle, CategorySelectProps>(({ placeholder, initialCategoryIds, onChange }, ref) => {
    const { t } = useTranslation()
    const { categoriesTree } = useStoreContext()

    const [isOpen, setIsOpen] = useState(false)
    const [appliedIds, setAppliedIds] = useState<string[]>([])
    const [draftIds, setDraftIds] = useState<ReadonlySet<string>>(new Set())
    const [expandedIds, setExpandedIds] = useState<ReadonlySet<string>>(new Set())

    const label = appliedIds.length > 0 ? t("common:categories", { count: appliedIds.length }) : placeholder

    const isLimitReached = useMemo(
      () => collectTopmostSelectedIds(categoriesTree ?? [], draftIds).length >= MAX_SELECTED_CATEGORIES,
      [categoriesTree, draftIds],
    )

    const { context, floatingStyles, refs } = useFloating({
      middleware: [offset(4)],
      open: isOpen,
      placement: "bottom-end",
      whileElementsMounted: autoUpdate,
      onOpenChange: setIsOpen,
    })
    const dismiss = useDismiss(context, {
      ancestorScroll: true,
      outsidePress: event => event.target !== document.documentElement, // Clicking a native scrollbar reports the target as <html>; treat that as an inside press.
    })
    const click = useClick(context, { toggle: true })
    const hover = useHover(context, { handleClose: safePolygon() })
    const role = useRole(context)
    const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, click, hover, role])

    const { subtreeIdsById, ancestorIdsById } = useMemo(() => {
      const subtreeIdsById = new Map<string, string[]>()
      const ancestorIdsById = new Map<string, string[]>()

      const visit = (category: CategoryParentBaseWithChildren, ancestorIds: string[]): string[] => {
        ancestorIdsById.set(category.id, ancestorIds)
        const childAncestorIds = [category.id, ...ancestorIds]
        const ids = [category.id, ...category.children.flatMap(child => visit(child, childAncestorIds))]
        subtreeIdsById.set(category.id, ids)
        return ids
      }

      categoriesTree?.forEach(category => visit(category, []))
      return { subtreeIdsById, ancestorIdsById }
    }, [categoriesTree])

    // The tree arrives asynchronously and reads as an empty array until then, so wait for the maps to actually fill up.
    const isSeeded = useRef(false)

    useEffect(() => {
      if (isSeeded.current || subtreeIdsById.size === 0) return
      isSeeded.current = true

      const ids = initialCategoryIds?.filter(id => subtreeIdsById.has(id)).slice(0, MAX_SELECTED_CATEGORIES)
      if (!ids?.length) return

      setAppliedIds(ids)
      setDraftIds(new Set(ids.flatMap(id => subtreeIdsById.get(id) ?? [])))
      // Expand the ancestors, otherwise a restored deep selection stays hidden behind collapsed parents.
      setExpandedIds(new Set(ids.flatMap(id => ancestorIdsById.get(id) ?? [])))
    }, [ancestorIdsById, initialCategoryIds, subtreeIdsById])

    const clearSelection = useCallback(() => {
      setAppliedIds([])
      setDraftIds(new Set())
    }, [])

    // Resetting from the outside only clears the selection: the caller already drives its own state, so onChange stays silent.
    useImperativeHandle(ref, () => ({ reset: clearSelection }), [clearSelection])

    const handleToggleSelect = useCallback(
      (id: string) =>
        setDraftIds(prev => {
          const subtreeIds = subtreeIdsById.get(id) ?? [id]
          const next = new Set(prev)
          if (prev.has(id)) {
            // Unchecking drops the whole subtree and every ancestor — a parent stays checked only while all of its descendants are.
            subtreeIds.forEach(i => next.delete(i))
            ancestorIdsById.get(id)?.forEach(i => next.delete(i))
          } else {
            if (isLimitReached) return prev
            subtreeIds.forEach(i => next.add(i))
          }
          return next
        }),
      [ancestorIdsById, isLimitReached, subtreeIdsById],
    )

    const handleToggleExpand = useCallback(
      (id: string) =>
        setExpandedIds(prev => {
          const next = new Set(prev)
          if (!next.delete(id)) next.add(id)
          return next
        }),
      [],
    )

    const handleApply = useCallback(() => {
      const ids = categoriesTree ? collectTopmostSelectedIds(categoriesTree, draftIds) : []
      setAppliedIds(ids)
      setIsOpen(false)
      onChange?.(ids.length > 0 ? ids : undefined)
    }, [categoriesTree, draftIds, onChange])

    const handleReset = useCallback(() => {
      clearSelection()
      setIsOpen(false)
      onChange?.(undefined)
    }, [clearSelection, onChange])

    if (!categoriesTree) return null

    return (
      <>
        <div
          ref={refs.setReference}
          className={twMerge(
            "box-border flex h-10 w-37.5 cursor-pointer items-center justify-between gap-2 rounded border bg-gray-100 py-2 pl-3.5 pr-1.5 hover:border-gray-400",
            isOpen ? "border-gray-400" : "border-gray-300",
          )}
          {...getReferenceProps()}
        >
          <span className="truncate text-2sm font-medium leading-4 text-gray-800">{label}</span>
          <SvgDropdownIndicator className="shrink-0 stroke-gray-500" />
        </div>
        {isOpen && (
          <FloatingPortal>
            <div
              ref={refs.setFloating}
              className="z-60 flex w-75 flex-col gap-3 rounded border border-gray-300 bg-[#FCFCFC] p-3 shadow-md"
              style={floatingStyles}
              {...getFloatingProps()}
            >
              <div className="flex max-h-84.5 flex-col gap-0.5 overflow-y-auto rounded border border-gray-300 bg-white">
                {categoriesTree.map(category => (
                  <CategorySelectItem
                    key={category.id}
                    category={category}
                    depth={0}
                    selectedIds={draftIds}
                    expandedIds={expandedIds}
                    isLimitReached={isLimitReached}
                    onToggleSelect={handleToggleSelect}
                    onToggleExpand={handleToggleExpand}
                  />
                ))}
              </div>
              <div className="flex items-center justify-between gap-2">
                <ResetAllButton onClick={handleReset} disabled={draftIds.size === 0} />
                <ButtonPrimary className="px-6 py-2" label={t("common:apply")} onClick={handleApply} />
              </div>
            </div>
          </FloatingPortal>
        )}
      </>
    )
  }),
)
