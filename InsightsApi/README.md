```
{
  users
  {
    firstName
    age
    lastName
    passport {id number}
  }
}
```

```
{
  users @aggregation(by: "firstName")
  {
    firstName
    age @max
    lastName @max
    passport @max(by: "passportId") {id number}
  }
}
```
