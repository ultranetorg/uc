import { useAuthenticationContext } from "app"
import { SkeletonBlock } from "ui/components"

export const StoreHeaderSkeleton = () => {
  const { selectedUserName } = useAuthenticationContext()

  return (
    <div className="flex flex-col gap-4 pb-6 md:flex-row md:items-center md:justify-between md:gap-8 md:pb-8">
      <div className="flex w-full min-w-0 items-center justify-between gap-4 md:max-w-135 md:flex-1">
        <div className="box-border flex h-11 w-full items-center justify-between gap-3 rounded-sm border border-gray-300 bg-gray-100 pr-3 pl-2.5">
          <SkeletonBlock className="h-4 w-24" rounded="xs" />
          <SkeletonBlock className="size-5 shrink-0" rounded="full" />
        </div>
      </div>
      <div className="flex shrink-0 items-center justify-end gap-6 md:gap-8">
        {selectedUserName && (
          <>
            <div className="flex h-6 items-center">
              <SkeletonBlock className="h-3.75 w-16" rounded="xs" />
            </div>
            <div className="flex h-6 items-center">
              <SkeletonBlock className="h-3.75 w-12" rounded="xs" />
            </div>
          </>
        )}
      </div>
    </div>
  )
}
