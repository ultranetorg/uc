import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { CommentsListSkeleton, SkeletonBlock, SkeletonText } from "ui/components"

const INFO_FIELDS_COUNT = 5
const LICENSE_FIELD_INDEX = 1
const THUMBS_COUNT = 7
const DESCRIPTION_LINES_COUNT = 4
const DOWNLOAD_BUTTONS_COUNT = 4
const REQUIREMENTS_LINES_COUNT = 5

type InfoFieldProps = PropsWithChildren & { labelClassName: string }

const InfoField = ({ labelClassName, children }: InfoFieldProps) => (
  <div className="flex flex-col gap-2">
    <SkeletonBlock className={twMerge("my-0.5 h-3", labelClassName)} rounded="xs" />
    {children}
  </div>
)

const InfoValue = ({ className }: { className: string }) => (
  <SkeletonBlock className={twMerge("my-0.5 h-4", className)} rounded="xs" />
)

const SoftwareInfoSkeleton = () => (
  <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <InfoField labelClassName="w-20">
      <div className="flex items-center gap-2">
        <SkeletonBlock className="size-8 shrink-0" rounded="full" />
        <SkeletonBlock className="h-4 w-40" rounded="xs" />
      </div>
    </InfoField>
    <InfoField labelClassName="w-20">
      <SkeletonBlock className="h-10 w-full" rounded="sm" />
    </InfoField>
    {Array.from({ length: INFO_FIELDS_COUNT }, (_, i) => (
      <InfoField key={i} labelClassName="w-20">
        <InfoValue className={twMerge("w-36", i === LICENSE_FIELD_INDEX && "my-1")} />
      </InfoField>
    ))}
    <InfoField labelClassName="w-20">
      <div className="flex flex-col">
        <InfoValue className="w-full" />
        <InfoValue className="w-full" />
      </div>
    </InfoField>
    <div className="flex flex-col gap-4">
      {Array.from({ length: DOWNLOAD_BUTTONS_COUNT }, (_, i) => (
        <SkeletonBlock key={i} className="h-11 w-full" rounded="sm" />
      ))}
    </div>
  </div>
)

const SystemRequirementsSkeleton = () => (
  <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <SkeletonBlock className="h-6 w-48" rounded="xs" />
    <div className="flex h-6 items-center gap-6">
      <SkeletonBlock className="h-4 w-16" rounded="xs" />
      <SkeletonBlock className="h-4 w-16" rounded="xs" />
    </div>
    <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
      {[0, 1].map(column => (
        <div key={column} className="flex flex-col gap-4">
          <SkeletonBlock className="my-0.5 h-3.5 w-24" rounded="xs" />
          <div className="flex flex-col gap-2">
            {Array.from({ length: REQUIREMENTS_LINES_COUNT }, (_, i) => (
              <SkeletonBlock key={i} className="my-0.5 h-3.5 w-1/2" rounded="xs" />
            ))}
          </div>
        </div>
      ))}
    </div>
  </div>
)

export const PublicationPageSkeleton = () => (
  <div className="flex flex-col gap-6">
    <div className="flex items-center gap-4">
      <SkeletonBlock className="size-17 shrink-0" rounded="2xl" />
      <SkeletonBlock className="h-8 w-72" rounded="sm" />
    </div>
    <div className="flex gap-8">
      <div className="flex min-w-187.5 flex-1 flex-col gap-8">
        <div className="flex max-w-187.5 flex-col gap-3">
          <SkeletonBlock className="h-104 w-full" rounded="lg" />
          <div className="flex gap-2">
            {Array.from({ length: THUMBS_COUNT }, (_, i) => (
              <SkeletonBlock key={i} className="h-15 w-24 shrink-0" rounded="md" />
            ))}
          </div>
        </div>
        <div className="divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
          <div className="flex flex-col gap-6 p-6">
            <div className="flex items-center justify-between">
              <SkeletonBlock className="h-6 w-32" rounded="xs" />
              <SkeletonBlock className="h-10 w-40" rounded="sm" />
            </div>
            <SkeletonText lines={DESCRIPTION_LINES_COUNT} className="py-1" />
          </div>
          <div className="flex justify-center py-4">
            <SkeletonBlock className="h-5 w-24" rounded="xs" />
          </div>
        </div>
        <SystemRequirementsSkeleton />
        <div className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <SkeletonBlock className="h-6 w-20" rounded="xs" />
            <SkeletonBlock className="h-9.5 w-35" rounded="sm" />
          </div>
          <CommentsListSkeleton count={2} />
        </div>
      </div>
      <div className="flex w-87.5 flex-col gap-8">
        <SoftwareInfoSkeleton />
        <SkeletonBlock className="h-14.5 w-full" rounded="lg" />
        <SkeletonBlock className="h-13 w-full" rounded="lg" />
      </div>
    </div>
  </div>
)
