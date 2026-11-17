export const getFavoriteStoreVisibilityClassName = (index: number): string =>
  index < 3 ? "hidden sm:block" : index < 6 ? "hidden md:block" : "hidden lg:block"
