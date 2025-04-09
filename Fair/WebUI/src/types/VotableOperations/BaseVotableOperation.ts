export type BaseVotableOperation = {
  $type:
    | "nickname-change"
    | "publication-product-change"
    | "publication-status-change"
    | "publication-update-moderation"
    | "review-status-change"
    | "review-text-moderation"
    | "site-authors-change"
    | "site-moderators-change"
    | "site-policy-change"
}
