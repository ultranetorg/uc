type Proposal = {
  change: string
  first: object
  second: object
  text: string
}

export type ModeratorDispute = {
  id: string
  flags: string[]
  proposal: Proposal
  pros: string[]
  cons: string[]
  expiration: number
}
