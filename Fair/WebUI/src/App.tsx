import { useGetWeather } from "./entities"

function App() {
  const { data, isPending } = useGetWeather()

  if (isPending) {
    return <h1>Loading...</h1>
  }

  return (
    <>
      <h1 className="text-3xl font-bold underline">Hello world!</h1>
      {JSON.stringify(data)}
    </>
  )
}

export default App
