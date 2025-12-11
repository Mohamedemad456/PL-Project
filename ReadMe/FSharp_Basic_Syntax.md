<!-- Role: Documentation Lead -->
<!-- Developer: Omar Salama -->
# F# Basic Syntax Guide

This document provides a beginner-friendly introduction to basic F# syntax, covering variables, loops, functions, and other fundamental concepts.

## Table of Contents

1. [Variables and Values](#variables-and-values)
2. [Data Types](#data-types)
3. [Functions](#functions)
4. [Control Flow](#control-flow)
5. [Loops](#loops)
6. [Lists and Collections](#lists-and-collections)
7. [Pattern Matching](#pattern-matching)
8. [Tuples](#tuples)
9. [Records](#records)
10. [Option Types](#option-types)

---

## Variables and Values

### Immutable Values (let bindings)

In F#, you use `let` to bind values. By default, values are **immutable** (cannot be changed):

```fsharp
// Simple value binding
let name = "John"
let age = 25
let pi = 3.14159

// Type inference - F# automatically determines the type
let number = 42        // int
let price = 19.99     // float
let text = "Hello"    // string
```

### Mutable Variables

If you need to change a value, use `mutable`:

```fsharp
let mutable counter = 0
counter <- counter + 1  // Use <- to assign to mutable variables
printfn "Counter: %d" counter  // Output: Counter: 1
```

### Type Annotations

You can explicitly specify types:

```fsharp
let name: string = "John"
let age: int = 25
let price: float = 19.99
```

---

## Data Types

### Basic Types

```fsharp
// Integers
let intValue: int = 42
let int64Value: int64 = 100L
let byteValue: byte = 255uy

// Floating Point
let floatValue: float = 3.14
let decimalValue: decimal = 99.99m

// Boolean
let isTrue: bool = true
let isFalse: bool = false

// String
let message: string = "Hello, F#!"
let multiline = """
This is a
multiline string
"""

// Character
let charValue: char = 'A'
```

### String Operations

```fsharp
let firstName = "John"
let lastName = "Doe"

// Concatenation
let fullName = firstName + " " + lastName

// String interpolation (F# 5.0+)
let greeting = $"Hello, {firstName} {lastName}!"

// String formatting
let formatted = sprintf "Name: %s, Age: %d" firstName 25
```

---

## Functions

### Basic Function Definition

```fsharp
// Simple function
let add x y = x + y

// Usage
let result = add 5 3  // Result: 8
```

### Function with Type Annotations

```fsharp
// Explicit types
let multiply (x: int) (y: int): int = x * y

// Usage
let product = multiply 4 5  // Result: 20
```

### Functions with Multiple Statements

```fsharp
let calculateTotal price quantity =
    let subtotal = price * quantity
    let tax = subtotal * 0.08
    subtotal + tax  // Last expression is the return value

// Usage
let total = calculateTotal 10.0 3  // Result: 32.4
```

### Lambda Functions (Anonymous Functions)

```fsharp
// Lambda function
let square = fun x -> x * x

// Usage
let result = square 5  // Result: 25

// Inline lambda
let numbers = [1; 2; 3; 4; 5]
let squares = List.map (fun x -> x * x) numbers  // [1; 4; 9; 16; 25]
```

### Recursive Functions

Use `rec` keyword for recursive functions:

```fsharp
let rec factorial n =
    if n <= 1 then
        1
    else
        n * factorial (n - 1)

// Usage
let fact5 = factorial 5  // Result: 120
```

### Higher-Order Functions

Functions that take other functions as parameters:

```fsharp
let applyOperation operation x y = operation x y

let add x y = x + y
let multiply x y = x * y

let result1 = applyOperation add 5 3      // Result: 8
let result2 = applyOperation multiply 4 5 // Result: 20
```

---

## Control Flow

### If-Then-Else

```fsharp
// Simple if-else
let max a b =
    if a > b then
        a
    else
        b

// If-else-if
let grade score =
    if score >= 90 then "A"
    elif score >= 80 then "B"
    elif score >= 70 then "C"
    else "F"
```

### When Expression

```fsharp
let result =
    if x > 0 then "Positive"
    elif x < 0 then "Negative"
    else "Zero"
```

---

## Loops

### For Loop (Iterating over a range)

```fsharp
// Simple for loop
for i = 1 to 10 do
    printfn "Number: %d" i

// For loop with step
for i = 0 to 10 do
    printfn "Even: %d" (i * 2)

// Downward iteration
for i = 10 downto 1 do
    printfn "Countdown: %d" i
```

### For-In Loop (Iterating over collections)

```fsharp
// Iterate over a list
let numbers = [1; 2; 3; 4; 5]
for number in numbers do
    printfn "Number: %d" number

// Iterate over a string
let text = "Hello"
for char in text do
    printfn "Character: %c" char

// Iterate over an array
let names = [|"Alice"; "Bob"; "Charlie"|]
for name in names do
    printfn "Name: %s" name
```

### While Loop

```fsharp
let mutable counter = 0
while counter < 5 do
    printfn "Counter: %d" counter
    counter <- counter + 1
```

### List Comprehensions (Alternative to loops)

```fsharp
// Generate a list
let squares = [for i in 1..10 -> i * i]
// Result: [1; 4; 9; 16; 25; 36; 49; 64; 81; 100]

// With condition
let evens = [for i in 1..20 do if i % 2 = 0 then yield i]
// Result: [2; 4; 6; 8; 10; 12; 14; 16; 18; 20]
```

---

## Lists and Collections

### Lists

```fsharp
// Creating lists
let numbers = [1; 2; 3; 4; 5]
let names = ["Alice"; "Bob"; "Charlie"]

// Empty list
let empty = []

// List with range
let range = [1..10]  // [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]

// List operations
let first = List.head numbers      // 1
let rest = List.tail numbers       // [2; 3; 4; 5]
let length = List.length numbers   // 5
let reversed = List.rev numbers    // [5; 4; 3; 2; 1]

// Adding elements
let newList = 0 :: numbers  // [0; 1; 2; 3; 4; 5] (prepend)
let combined = numbers @ [6; 7]  // [1; 2; 3; 4; 5; 6; 7] (append)
```

### Arrays

```fsharp
// Creating arrays
let numbers = [|1; 2; 3; 4; 5|]
let names = [|"Alice"; "Bob"|]

// Accessing elements
let first = numbers.[0]  // 1
numbers.[0] <- 10  // Mutable arrays can be modified

// Array operations
let length = Array.length numbers
let reversed = Array.rev numbers
```

### Sequences

```fsharp
// Creating sequences
let numbers = seq {1..10}
let squares = seq {for i in 1..10 -> i * i}

// Lazy evaluation - only computed when needed
let infinite = seq {1..System.Int32.MaxValue}
```

### Common List/Array Operations

```fsharp
let numbers = [1; 2; 3; 4; 5]

// Map - transform each element
let doubled = List.map (fun x -> x * 2) numbers  // [2; 4; 6; 8; 10]

// Filter - keep elements that match condition
let evens = List.filter (fun x -> x % 2 = 0) numbers  // [2; 4]

// Fold - accumulate a value
let sum = List.fold (fun acc x -> acc + x) 0 numbers  // 15

// Find - find first matching element
let found = List.find (fun x -> x > 3) numbers  // 4

// Exists - check if any element matches
let hasEven = List.exists (fun x -> x % 2 = 0) numbers  // true
```

---

## Pattern Matching

Pattern matching is a powerful feature in F#:

```fsharp
// Match on values
let describe number =
    match number with
    | 0 -> "Zero"
    | 1 -> "One"
    | 2 -> "Two"
    | _ -> "Other"  // Default case

// Match on conditions
let grade score =
    match score with
    | s when s >= 90 -> "A"
    | s when s >= 80 -> "B"
    | s when s >= 70 -> "C"
    | _ -> "F"

// Match on lists
let describeList list =
    match list with
    | [] -> "Empty list"
    | [x] -> sprintf "Single element: %d" x
    | [x; y] -> sprintf "Two elements: %d, %d" x y
    | _ -> "Many elements"

// Match on tuples
let describePoint point =
    match point with
    | (0, 0) -> "Origin"
    | (x, 0) -> sprintf "On X-axis at %d" x
    | (0, y) -> sprintf "On Y-axis at %d" y
    | (x, y) -> sprintf "Point at (%d, %d)" x y
```

---

## Tuples

Tuples group multiple values together:

```fsharp
// Creating tuples
let person = ("John", 25, "Engineer")
let point = (10, 20)

// Accessing tuple elements
let name, age, job = person
let x, y = point

// Named tuples (F# 4.1+)
let person2 = (name = "John", age = 25, job = "Engineer")
let name2 = person2.name  // "John"
```

---

## Records

Records are like simple classes:

```fsharp
// Define a record type
type Person = {
    Name: string
    Age: int
    Email: string
}

// Create a record
let person = {
    Name = "John"
    Age = 25
    Email = "john@example.com"
}

// Access fields
let name = person.Name
let age = person.Age

// Create a copy with changes
let olderPerson = { person with Age = 26 }
```

---

## Option Types

Option types represent values that might not exist:

```fsharp
// Some value
let someValue = Some 42

// No value
let noValue = None

// Pattern matching on options
let describeOption opt =
    match opt with
    | Some value -> sprintf "Has value: %d" value
    | None -> "No value"

// Usage
let result1 = describeOption (Some 42)  // "Has value: 42"
let result2 = describeOption None       // "No value"

// Common operations
let numbers = [Some 1; None; Some 3; Some 4]

// Get all Some values
let values = List.choose id numbers  // [1; 3; 4]

// Check if option has value
let hasValue = Option.isSome (Some 42)  // true
let hasNoValue = Option.isNone None     // true
```

---

## Practical Examples

### Example 1: Simple Calculator Function

```fsharp
let calculate operation a b =
    match operation with
    | "+" -> a + b
    | "-" -> a - b
    | "*" -> a * b
    | "/" -> if b <> 0 then a / b else failwith "Division by zero"
    | _ -> failwith "Unknown operation"

let result = calculate "+" 10 5  // 15
```

### Example 2: Processing a List

```fsharp
let numbers = [1..10]

// Get sum of even numbers
let sumOfEvens =
    numbers
    |> List.filter (fun x -> x % 2 = 0)
    |> List.sum

// Get squares of numbers greater than 5
let squares =
    numbers
    |> List.filter (fun x -> x > 5)
    |> List.map (fun x -> x * x)
```

### Example 3: Working with Records

```fsharp
type Book = {
    Title: string
    Author: string
    Year: int
}

let books = [
    { Title = "1984"; Author = "George Orwell"; Year = 1949 }
    { Title = "To Kill a Mockingbird"; Author = "Harper Lee"; Year = 1960 }
    { Title = "The Great Gatsby"; Author = "F. Scott Fitzgerald"; Year = 1925 }
]

// Find books after 1950
let modernBooks =
    books
    |> List.filter (fun book -> book.Year > 1950)
```

---

## Key Differences from C#/Java

1. **Immutability**: Values are immutable by default
2. **Type Inference**: Types are often inferred automatically
3. **No Semicolons**: Statements don't need semicolons
4. **Indentation Matters**: Code structure is defined by indentation
5. **Expressions, not Statements**: Everything is an expression that returns a value
6. **Pattern Matching**: Powerful pattern matching instead of just if-else
7. **Pipes**: Use `|>` to chain operations

---

## Quick Reference

### Variable Declaration
```fsharp
let x = 10                    // Immutable
let mutable y = 20            // Mutable
y <- 30                       // Assign to mutable
```

### Function Definition
```fsharp
let add x y = x + y           // Simple
let multiply (x: int) (y: int): int = x * y  // With types
let rec factorial n = ...     // Recursive
```

### Loops
```fsharp
for i = 1 to 10 do ...       // For loop
for item in list do ...      // For-in loop
while condition do ...       // While loop
```

### Lists
```fsharp
let list = [1; 2; 3]         // Create
let head = List.head list    // First element
let tail = List.tail list    // Rest
let mapped = List.map f list // Transform
```

### Pattern Matching
```fsharp
match value with
| pattern1 -> result1
| pattern2 -> result2
| _ -> default
```

---

## Next Steps

- Learn about **modules** and **namespaces**
- Explore **discriminated unions**
- Study **async/await** for asynchronous programming
- Practice **pipeline operators** (`|>` and `<|`)
- Understand **computation expressions**

---

**Last Updated:** December 2024  
**Maintained By:** Omar Salama (Documentation Lead)
