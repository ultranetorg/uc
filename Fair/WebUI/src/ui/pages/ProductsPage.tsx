import { ChangeEvent, useCallback } from "react"

import { Button, SearchInput } from "ui/components"

export const ProductsPage = () => {
  const handleChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {
    console.log(e.target.value)
  }, [])

  const handleClick = useCallback(() => {
    console.log("a")
  }, [])

  return (
    <div>
      <h1>ProductsPage</h1>
      <SearchInput onChange={handleChange} />
      <Button onClick={handleClick} value="Search" />
    </div>
  )
}
