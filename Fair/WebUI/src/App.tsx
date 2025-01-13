import { useState } from "react"

import { useGetWeather } from "./entities"
import "./App.css"

function App() {
  const [count, setCount] = useState(0)

  const { data, isPending } = useGetWeather()

  if (isPending) {
    return <h1>Loading...</h1>
  }

  return (
    <>
      <div></div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount(count => count + 1)}>count is {count}</button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="read-the-docs">Click on the Vite and React logos to learn more</p>
      {JSON.stringify(data)}
    </>
  )
}

export default App
