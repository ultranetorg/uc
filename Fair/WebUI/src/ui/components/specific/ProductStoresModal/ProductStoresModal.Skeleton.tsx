import { memo } from "react"

import { SvgX } from "assets"
import { Modal, ModalProps, SkeletonBlock } from "ui/components"

const ROWS_COUNT = 1

export type ProductStoresModalSkeletonProps = Pick<ModalProps, "onClose">

export const ProductStoresModalSkeleton = memo(({ onClose }: ProductStoresModalSkeletonProps) => (
  <Modal className="max-h-142.25 w-120 border border-gray-300 p-0">
    <div className="divide-y divide-gray-300">
      <div className="flex items-center justify-between gap-6 px-6 py-4">
        <SkeletonBlock className="my-0.5 h-4 w-40" rounded="xs" />
        <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onClose} />
      </div>
      <div className="p-4">
        <div className="flex items-center gap-3 p-2 text-2xs leading-4.25 font-medium select-none">
          <span className="w-[56%]">Store</span>
        </div>
        <div className="max-h-112 overflow-hidden">
          {Array.from({ length: ROWS_COUNT }, (_, i) => (
            <div key={i} className="flex items-center gap-3 p-2">
              <SkeletonBlock className="size-10 shrink-0" rounded="lg" />
              <SkeletonBlock className="my-0.5 h-4 w-4/5" rounded="xs" />
            </div>
          ))}
        </div>
      </div>
    </div>
  </Modal>
))
