export type SiteCardProps = {
  title: string
  description?: string
}

export const SiteCard = ({ title, description }: SiteCardProps) => (
  <div className="flex flex-col items-center gap-4 rounded-2xl border border-transparent px-2 py-6 hover:border-zinc-800">
    <div className="h-20 w-20 rounded-2xl bg-zinc-700" />
    <div className="flex h-14 w-52 flex-col items-center gap-[6px]">
      <span className="text-ellipsis text-[15px] font-semibold leading-[18px]">{title}</span>
      {description && <span className="text-ellipsis text-center text-[13px] leading-4">{description}</span>}
    </div>
  </div>
)
