export type ProposalInfoProps = {
  title: string
  text?: string
}

export const ProposalInfo = ({ title, text }: ProposalInfoProps) => (
  <div className="flex h-fit w-full max-w-187.5 flex-col divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
    <div className="flex flex-col gap-4 p-6">
      <span
        className="overflow-hidden text-ellipsis whitespace-nowrap text-2base font-semibold leading-5.25"
        title={title}
      >
        {title}
      </span>
      {text && <span className="text-2sm leading-5">{text}</span>}
    </div>
    <div className="p-6">Action</div>
  </div>
)
