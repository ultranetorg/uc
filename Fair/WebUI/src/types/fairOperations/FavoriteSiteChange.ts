import { BaseFairOperation } from "./BaseFairOperation"

export class FavoriteSiteChange extends BaseFairOperation {
  public site: string
  public action: boolean

  constructor(site: string, action: boolean) {
    super("FavoriteSiteChange")
    this.site = site
    this.action = action
  }
}
