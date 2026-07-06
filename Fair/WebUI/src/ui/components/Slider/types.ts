export type SliderItem = {
  id?: string | number
  src: string
  poster?: string
  alt?: string
}

export type SliderProps = {
  items?: SliderItem[]
}
