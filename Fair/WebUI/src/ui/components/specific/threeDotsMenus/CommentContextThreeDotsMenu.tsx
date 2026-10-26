import { memo, useCallback, useMemo, useState } from "react"
import {
  FloatingPortal,
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useStoreRolesContext, useUserContext } from "app"
import { SvgThreeDotsVertical } from "assets"
import { useResolveStoreId, useScrollOrResize } from "hooks"
import { CommentContextMenuProps, SimpleMenu, SimpleMenuItem } from "ui/components"
import { routes } from "utils"

import { useUserDetailsThreeDotsMenu } from "./UserDetailsThreeDotsMenu"

type CommentContextMenuBaseProps = {
  onEditReview: (id: string, text: string) => void
  onDeleteReview: (id: string, text: string) => void
}

export type CommentContextThreeDotsMenu = CommentContextMenuProps & CommentContextMenuBaseProps

export const CommentContextThreeDotsMenu = memo(
  ({
    id,
    publicationId,
    text,
    reviewerId,
    reviewerName,
    onEditReview,
    onDeleteReview,
  }: CommentContextThreeDotsMenu) => {
    const { t } = useTranslation("commentContextMenu")

    const storeId = useResolveStoreId()
    const { isModerator } = useStoreRolesContext()
    const { user } = useUserContext()
    const moderatorMenuItems = useUserDetailsThreeDotsMenu(reviewerId, reviewerName)

    const [isExpanded, setExpanded] = useState(false)

    useScrollOrResize(() => setExpanded(false), isExpanded)

    const { context, floatingStyles, refs } = useFloating({
      middleware: [offset(4)],
      open: isExpanded,
      placement: "bottom-end",
      onOpenChange: setExpanded,
    })
    const dismiss = useDismiss(context)
    const hover = useHover(context, { handleClose: safePolygon() })
    const role = useRole(context)
    const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

    const handleEditClick = useCallback(() => {
      setExpanded(false)
      onEditReview(id, text)
    }, [id, onEditReview, text])

    const handleDeleteClick = useCallback(() => {
      setExpanded(false)
      onDeleteReview(id, text)
    }, [id, onDeleteReview, text])

    const canEdit = user && user.id === reviewerId

    const menuItems = useMemo<SimpleMenuItem[]>(
      () => [
        ...(canEdit
          ? [
              {
                onClick: handleEditClick,
                label: t("common:edit"),
              },
            ]
          : []),
        ...(canEdit && !isModerator
          ? [
              {
                onClick: handleDeleteClick,
                label: t("deleteReview"),
              },
            ]
          : []),
        ...(isModerator
          ? [
              ...(canEdit ? [{ separator: true }] : []),
              {
                label: t("deleteReview"),
                to: routes.moderation.createProposal(storeId!),
                state: {
                  title: truncate(`Delete ${reviewerName} review`, { length: 64 }),
                  type: "review-deletion",
                  reviewId: id,
                  publicationId: publicationId,
                  parentBreadcrumbs: [
                    { path: routes.moderation.reviews(storeId!, "deletions"), title: t("common:reviews") },
                  ],
                  redirectAfterProposalCreation: routes.moderation.reviews(storeId!, "deletions"),
                  redirectAfterProposalExecution: routes.publication(publicationId),
                },
              },
              ...moderatorMenuItems,
            ]
          : []),
      ],
      [
        canEdit,
        handleDeleteClick,
        handleEditClick,
        id,
        isModerator,
        moderatorMenuItems,
        publicationId,
        reviewerName,
        storeId,
        t,
      ],
    )

    const handleMenuClick = useCallback(() => setExpanded(false), [])

    if (!menuItems.length) return null

    return (
      <>
        <div ref={refs.setReference} {...getReferenceProps()}>
          <SvgThreeDotsVertical className="cursor-pointer fill-gray-500 hover:fill-gray-800" />
        </div>
        {isExpanded && (
          <FloatingPortal>
            <SimpleMenu
              ref={refs.setFloating}
              className="w-auto"
              items={menuItems}
              style={floatingStyles}
              onClick={handleMenuClick}
              {...getFloatingProps()}
            />
          </FloatingPortal>
        )}
      </>
    )
  },
)
