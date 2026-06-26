import { memo, PropsWithChildren } from "react"

import { FullscreenPageView } from "ui/views"
import { BaseLayout, ConstrainedWidthLayout, SiteLayout } from "ui/layouts"

type MaybeFullscreenBaseProps = {
  showFullscreen?: boolean
}

export type MaybeFullscreenProps = PropsWithChildren & MaybeFullscreenBaseProps

export const MaybeFullscreen = memo(({ children, showFullscreen }: MaybeFullscreenProps) =>
  showFullscreen ? (
    <FullscreenPageView>{children}</FullscreenPageView>
  ) : (
    <BaseLayout>
      <SiteLayout>
        <ConstrainedWidthLayout>{children}</ConstrainedWidthLayout>
      </SiteLayout>
    </BaseLayout>
  ),
)
