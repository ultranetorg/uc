export type SiteCardProps = {
  title: string
  description?: string
}

export const SiteCard = ({ title, description }: SiteCardProps) => (
  <div className="flex flex-col items-center gap-4 rounded-2xl border border-transparent px-2 py-6 hover:border-zinc-800">
    <div className="h-20 w-20 rounded-2xl bg-zinc-700" />
    <div className="flex w-full flex-col gap-[6px]">
      <span className="h-[18px] w-52 overflow-hidden text-ellipsis whitespace-nowrap text-center text-[15px] font-semibold leading-[18px]">
        {title}
      </span>
      {description && (
        <span className="line-clamp-2 h-8 w-52 overflow-hidden text-center text-[13px] leading-4">{description}</span>
      )}
    </div>
  </div>
)
