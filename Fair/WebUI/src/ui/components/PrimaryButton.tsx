import { twMerge } from "tailwind-merge"

import { Button, ButtonProps } from "./Button"

export const PrimaryButton = ({ className, label, image, onClick }: ButtonProps) => (
  <Button
    className={twMerge("rounded-lg border border-neutral-800", className)}
    label={label}
    image={image}
    onClick={onClick}
  />
)
