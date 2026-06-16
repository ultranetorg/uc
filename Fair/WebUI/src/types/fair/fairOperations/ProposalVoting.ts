import { BaseFairOperation } from "./BaseFairOperation"

export class ProposalVoting extends BaseFairOperation {
  public proposal: string
  public voter: string
  public choice: number

  constructor(proposal: string, voter: string, choice: number) {
    super("ProposalVoting")
    this.proposal = proposal
    this.voter = voter
    this.choice = choice
  }
}
