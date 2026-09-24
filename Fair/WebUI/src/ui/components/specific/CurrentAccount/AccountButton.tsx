import { ComponentPropsWithoutRef, forwardRef, memo } from "react"

import avatarFallback3xl from "assets/fallback/user-10.png"
import { PropsWithClassName } from "types"
import { ImageFallback } from "ui/components"
import { buildUserAvatarByNameUrl } from "utils"

import { PanelButton } from "./PanelButton"

interface AccountButtonBaseProps {
  name: string
  avatarVersion?: number
}

export type AccountButtonProps = PropsWithClassName &
  AccountButtonBaseProps &
  Omit<ComponentPropsWithoutRef<"div">, "children" | "className">

export const AccountButton = memo(
  forwardRef<HTMLDivElement, AccountButtonProps>(({ name, avatarVersion, ...rest }, ref) => (
    <PanelButton
      iconBefore={
        <div className="size-10 shrink-0 overflow-hidden rounded-sm">
          <ImageFallback
            src={`${buildUserAvatarByNameUrl(name)}?v=${avatarVersion ?? 0}`}
            fallbackSrc={avatarFallback3xl}
          />
        </div>
      }
      ref={ref}
      {...rest}
    >
      <span className="text-2sm leading-4.5" title={name}>
        {name}
      </span>
    </PanelButton>
  )),
)
