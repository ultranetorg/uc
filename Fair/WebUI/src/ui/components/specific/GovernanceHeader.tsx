import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useModerationContext, useSiteContext } from "app"
import { ProposalType } from "types"
import { DropdownButton, SimpleMenuItem } from "ui/components"
import { getVisibleProposalOperations, groupOperations } from "utils"

import { ModerationHeader } from "./ModerationHeader"

export type GovernanceHeaderProps = {
  proposalType?: ProposalType
  title: string
  onCreateButtonClick?: () => void
  createButtonLabel?: string
}

export const GovernanceHeader = memo(
  ({ proposalType = "referendum", title, onCreateButtonClick, createButtonLabel }: GovernanceHeaderProps) => {
    const { t } = useTranslation()
    const { site } = useSiteContext()
    const navigate = useNavigate()

    const { isModerator, isPublisher, policies } = useModerationContext()

    const dropdownItems = useMemo<SimpleMenuItem[] | undefined>(() => {
      if (!site) return

      const operations = getVisibleProposalOperations(proposalType, policies)
      const grouped = groupOperations(operations)
      return grouped.flatMap<SimpleMenuItem>(({ items }, i) => {
        const mapped = items.map<SimpleMenuItem>(x => ({
          label: t(`operations:${x}`),
          onClick: () =>
            navigate(`/${site.id}/${proposalType === "discussion" ? "m" : "g"}/new`, { state: { type: x } }),
        }))

        if (i !== grouped.length - 1) mapped.push({ separator: true })

        return mapped
      })
    }, [navigate, policies, proposalType, site, t])

    return (
      <ModerationHeader
        title={title}
        components={
          <>
            {((proposalType === "referendum" && isPublisher) || (proposalType === "discussion" && isModerator)) &&
              dropdownItems &&
              dropdownItems.length > 0 &&
              onCreateButtonClick &&
              createButtonLabel && (
                <DropdownButton
                  className="first-letter:uppercase"
                  label={createButtonLabel}
                  items={dropdownItems!}
                  multiColumnMenu={false}
                  menuClassName="overflow-y-auto max-w-60 max-h-85"
                />
              )}
          </>
        }
      />
    )
  },
)
