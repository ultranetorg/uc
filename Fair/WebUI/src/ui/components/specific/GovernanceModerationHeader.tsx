import { Breadcrumbs2, ButtonPrimary } from "ui/components"

export type GovernanceModerationHeaderProps = {
  title: string
  totalItems?: number
  onCreateButtonClick?: () => void
  createButtonLabel?: string
}

export const GovernanceModerationHeader = ({
  title,
  totalItems,
  onCreateButtonClick,
  createButtonLabel,
}: GovernanceModerationHeaderProps) => (
  <div className="flex flex-col gap-2">
    <Breadcrumbs2 />
    <div className="flex justify-between">
      <div className="flex gap-2 text-3.5xl font-semibold leading-10">
        <span>{title}</span>
        {totalItems && <span className="text-gray-400">({totalItems})</span>}
      </div>
      {onCreateButtonClick && createButtonLabel && (
        <ButtonPrimary label={createButtonLabel} onClick={onCreateButtonClick} />
      )}
    </div>
  </div>
)
