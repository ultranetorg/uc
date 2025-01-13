import { WeatherForecast } from "types"

export type Api = {
  getWeathers: () => Promise<WeatherForecast>
}
