import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useSiteContext, useUserContext } from "app"
import { CREATE_DISCUSSION_EXTRA_OPERATION_TYPES } from "constants/"
import { ProposalType } from "types"
import {
  Breadcrumbs,
  BreadcrumbsItemProps,
  ButtonBar,
  ButtonOutline,
  DropdownButton,
  SimpleMenuItem,
} from "ui/components"
import { getVisibleProposalOperations, groupOperations } from "utils"

export type GovernanceModerationHeaderProps = {
  proposalType: ProposalType
  siteId: string
  title: string
  totalItems?: number
  parentBreadcrumb?: BreadcrumbsItemProps
  onCreateButtonClick?: () => void
  onSearchProduct?: () => void
  createButtonLabel?: string
  homeLabel: string
  searchProductLabel?: string
}

export const GovernanceModerationHeader = memo(
  ({
    proposalType,
    siteId,
    title,
    totalItems,
    parentBreadcrumb,
    onCreateButtonClick,
    onSearchProduct,
    createButtonLabel,
    homeLabel,
    searchProductLabel,
  }: GovernanceModerationHeaderProps) => {
    const { t } = useTranslation()
    const { site } = useSiteContext()
    const navigate = useNavigate()

    const { isModerator, isPublisher } = useUserContext()

    const dropdownItems = useMemo<SimpleMenuItem[] | undefined>(() => {
      if (!site) return

      const operations = getVisibleProposalOperations(
        proposalType,
        site.discussionOperations,
        site.referendumOperations,
      )
      const grouped = groupOperations(operations)
      return grouped.flatMap<SimpleMenuItem>(({ items }, i) => {
        const mapped = items.map<SimpleMenuItem>(x => ({
          label: !CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(x)
            ? t(`operations:${x}`)
            : t(`extraOperations:${x}`),
          onClick: () =>
            navigate(`/${site.id}/${proposalType === "discussion" ? "m" : "g"}/new`, { state: { type: x } }),
        }))

        if (i !== grouped.length - 1) mapped.push({ separator: true })

        return mapped
      })
    }, [navigate, proposalType, site, t])

    return (
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: homeLabel },
            ...(parentBreadcrumb ? [parentBreadcrumb] : []),
            { title: title },
          ]}
        />
        <div className="flex justify-between">
          <div className="flex gap-2 text-3.5xl font-semibold leading-10">
            <span>{title}</span>
            {totalItems && <span className="text-gray-400">({totalItems})</span>}
          </div>
          {((proposalType === "referendum" && isPublisher) || (proposalType === "discussion" && isModerator)) && (
            <ButtonBar>
              {onSearchProduct && searchProductLabel && (
                <ButtonOutline className="w-48" label={searchProductLabel} onClick={onSearchProduct} />
              )}
              {dropdownItems && dropdownItems.length > 0 && onCreateButtonClick && createButtonLabel && (
                <DropdownButton
                  label="Create referendum"
                  items={dropdownItems!}
                  multiColumnMenu={false}
                  menuClassName="overflow-y-auto max-w-60 max-h-85"
                />
              )}
            </ButtonBar>
          )}
        </div>
      </div>
    )
  },
)
