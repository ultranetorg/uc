import { BaseRate, PaginatedResponse } from "types"

import { Auction } from "./Auction"

export type Auctions = PaginatedResponse<Auction> & BaseRate
