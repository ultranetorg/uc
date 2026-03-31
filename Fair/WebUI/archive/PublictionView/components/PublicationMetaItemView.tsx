import React from "react"

export function PublicationMetaItemView({ children, label }: { children: React.ReactNode; label: string }) {
  return (
    <div className="flex flex-col gap-2">
      <span className="text-2xs text-gray-500">{label}</span>
      {children}
    </div>
  )
}

export function PublicationMetaItemViewSimple({ label, value }: { label: string; value: string | number }) {
  return (
    <PublicationMetaItemView label={label}>
      <span className="font-medium leading-4">{value}</span>
    </PublicationMetaItemView>
  )
}
