import { WeatherForecast } from "types"

import { Api } from "./Api"

const getWeathers = (): Promise<WeatherForecast> => {
  return fetch("https://localhost:7060/weatherforecast").then(res => res.json())
}

const api: Api = {
  getWeathers
}

export const getApi = () => api
