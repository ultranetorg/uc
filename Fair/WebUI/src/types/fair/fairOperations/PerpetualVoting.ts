import { BaseFairOperation } from "./BaseFairOperation"

export class PerpetualVoting extends BaseFairOperation {
  public store: string
  public referendum: number
  public publisher: string
  public choice: number

  constructor(store: string, referendum: number, publisher: string, choice: number) {
    super("PerpetualVoting")
    this.store = store
    this.referendum = referendum
    this.publisher = publisher
    this.choice = choice
  }
}
