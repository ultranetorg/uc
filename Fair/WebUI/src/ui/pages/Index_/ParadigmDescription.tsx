import { memo } from "react"

export const ParadigmDescription = memo(() => (
  <div className="flex w-135 flex-col gap-6">
    <span className="text-center text-2xl font-semibold leading-7.5">How New Paradigm Works</span>
    <ul className="list-inside list-disc space-y-5 text-2sm leading-4.5 text-gray-950">
      {/* @sync-start FairNode.WelcomeMessage */}
      <li>This is a decentralized platform of autonomous, transparent, community-governed stores.</li>
      <li>Anyone can become an author, create product pages, and publish them in the stores.</li>
      <li>An author has full control over their content and behavior.</li>
      <li>Stores can also be created by anyone and act as aggregators for product listings.</li>
      <li>
        Unlike authors, the creator of a store does not have full control over it — it is completely governed by all its
        members.
      </li>
      <li>A member of the store is an author who has products published in this store.</li>
      <li>Each time a new member joins the store, existing ones lose part of their leverage.</li>
      <li>
        Members vote for its governance policy, elect/recall moderators, and thus have full control over their store.
      </li>
      <li>
        Moderators are responsible for publishing product updates and other routine operations to maintain store content
        clean and tidy.
      </li>
      {/* @sync-end FairNode.WelcomeMessage */}
    </ul>
  </div>
))
