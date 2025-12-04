import React from "react"

type AsProp<C extends React.ElementType> = {
  as?: C
}

export type PropsWithAs<C extends React.ElementType> = AsProp<C> &
  Omit<React.ComponentPropsWithoutRef<C>, keyof AsProp<C>>
