import { ModerationHeaderSkeleton, ModerationPublicationSkeleton } from "ui/components/specific"

export const ModerationPublicationPageSkeleton = () => (
  <div className="flex flex-col gap-6">
    <ModerationHeaderSkeleton showActions />
    <ModerationPublicationSkeleton />
  </div>
)
