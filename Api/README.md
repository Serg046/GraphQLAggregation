```
{
  users
  {
    firstName
    lastName
    age
    kids
    passport {id number}
  }
}
```

```
{
  users @aggregation(by: "firstName")
  {
    firstName
    lastName @max
    age @avg
    kids @sum
    passport @min(by: "passportId") {id number}
  }
}
```
