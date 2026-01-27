import { BaseFairOperation } from "./BaseFairOperation"

export class PerpetualVoting extends BaseFairOperation {
  public site: string
  public referendum: number
  public publisher: string
  public choice: number

  constructor(site: string, referendum: number, publisher: string, choice: number) {
    super("PerpetualVoting")
    this.site = site
    this.referendum = referendum
    this.publisher = publisher
    this.choice = choice
  }
}
