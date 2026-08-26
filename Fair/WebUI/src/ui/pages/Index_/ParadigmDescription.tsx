import { memo } from "react"

export const ParadigmDescription = memo(() => (
  <div className="flex w-135 flex-col gap-6">
    <span className="text-center text-2xl font-semibold leading-7.5">How New Paradigm Works</span>
    <ul className="list-inside list-disc space-y-5 text-2sm leading-4.5 text-gray-950">
      <li>This is decentralized platform of autonomous transparent community-governed stores</li>
      <li>Anyone can become the author, create product pages and publish it in the stores</li>
      <li>Author has full control over its content and behavior</li>
      <li>The stores can also be created by anyone which act as aggregators for product listings</li>
      <li>
        Unlike authors, a creator of the store has no full control over it - it's completely governed by all its members
      </li>
      <li>A member of the store is a author who has products published in this store</li>
      <li>Each time a next member joins the store existing ones lose part of his leverage</li>
      <li>
        Members of the store votes for its governance policy, elect/recall moderators and thus has full control over
        their store
      </li>
      <li>
        Moderators responsible for publishing product updates and other routine operations to maintaining store content
        clean and tidy
      </li>
    </ul>
  </div>
))
