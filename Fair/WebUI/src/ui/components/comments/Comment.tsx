import { TEST_REVIEW_SRC } from "testConfig"
import { AccountBaseAvatar } from "types"
import { RatingBar } from "ui/components"
import { buildSrc, formatDate, shortenAddress } from "utils"

export type CommentProps = {
  account: AccountBaseAvatar
  created: number
  rating?: number
  text: string
}

export const Comment = ({ account, created, rating, text }: CommentProps) => (
  <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <div className="flex flex-col gap-4">
      <div className="flex gap-4.5">
        <div className="h-13 w-13 overflow-hidden rounded-full">
          <img src={buildSrc(account.avatar, TEST_REVIEW_SRC)} className="h-full w-full object-cover" />
        </div>
        <div className="flex flex-col justify-center gap-2">
          <span
            className="text-2sm font-semibold leading-4.5 text-gray-800"
            title={account.nickname || account.address}
          >
            {account.nickname || shortenAddress(account.address)}
          </span>
          <span className="text-2xs font-medium leading-4 text-gray-500">{formatDate(created)}</span>
        </div>
      </div>
      {rating && <RatingBar value={rating} />}
    </div>
    <div className="text-2sm leading-5 text-gray-800">{text}</div>
  </div>
)
