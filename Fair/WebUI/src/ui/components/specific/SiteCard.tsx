export type SiteCardProps = {
  title: string
  description?: string
}

export const SiteCard = ({ title, description }: SiteCardProps) => (
  <div className="flex w-52 flex-col items-center gap-4 rounded-2xl border border-transparent px-2 py-6 hover:border-zinc-800">
    <div className="h-20 w-20 rounded-2xl bg-zinc-700" />
    <div className="flex flex-col items-center gap-2">
      <span className="text-[15px] font-semibold">{title}</span>
      {description && <span className="text-center text-[13px]">{description}</span>}
    </div>
  </div>
)
