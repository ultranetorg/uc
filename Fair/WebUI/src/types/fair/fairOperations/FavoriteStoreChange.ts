import { BaseFairOperation } from "./BaseFairOperation"

export class FavoriteStoreChange extends BaseFairOperation {
  public store: string
  public action: boolean

  constructor(store: string, action: boolean) {
    super("FavoriteStoreChange")
    this.store = store
    this.action = action
  }
}
