import { memo } from "react"

export const ParadigmDescription = memo(() => (
  <div className="flex w-135 flex-col gap-6">
    <span className="text-center text-2xl font-semibold leading-7.5">How New Paradigm Works</span>
    <ul className="list-inside list-disc space-y-5 text-2sm leading-4.5 text-gray-950">
      <li>This is a decentralized platform of autonomous, transparent, and community-governed stores.</li>
      <li>Anyone can become an author, create product pages, and publish them in the stores.</li>
      <li>Authors retain full control over their content and its behavior.</li>
      <li>Anyone can also create a store, which acts as an aggregator of product listings.</li>
      <li>A store's creator has no special control over it; the store is governed entirely by its members.</li>
      <li>A store member is an author who has products published in that store.</li>
      <li>Each time a new member joins a store, existing members dilute a portion of their influence</li>
      <li>
        Members vote on the store's governance policies, elect or recall moderators, and thus retain full control over
        their store.
      </li>
      <li>
        Moderators are responsible for publishing product updates and handling routine operations to keep the store's
        content clean and tidy.
      </li>
    </ul>
  </div>
))
