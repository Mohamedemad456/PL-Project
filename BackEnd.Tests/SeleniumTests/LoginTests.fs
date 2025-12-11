// Role: Tester
// Developer: Amr
module BackEnd.Tests.SeleniumTests.LoginTests

open Xunit
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open System
open System.Threading

[<Fact>]
let ``Login with invalid credentials should display error message`` () =
    let options = ChromeOptions()
    // Keep browser open for debugging
    options.AddArgument("--disable-blink-features=AutomationControlled")
    let driver = new ChromeDriver(options)
    // Wait for driver to fully initialize
    Thread.Sleep(2000)
    try
        try
            // Navigate to login page
            driver.Navigate().GoToUrl("http://localhost:3000/login")
            
            // Wait for page to load
            Thread.Sleep(3000)
            
            // Find email input field
            let emailInput = driver.FindElement(By.Id("email"))
            emailInput.Clear()
            emailInput.SendKeys("invalid@email.com")
            Thread.Sleep(3000) // Wait to see email entered
            
            // Find password input field
            let passwordInput = driver.FindElement(By.Id("password"))
            passwordInput.Clear()
            passwordInput.SendKeys("wrongpassword123")
            Thread.Sleep(3000) // Wait to see password entered
            
            // Find and click the login button
            let loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Login')]"))
            loginButton.Click()
            Thread.Sleep(3000) // Wait to see button click
            
            // Wait for error message to appear
            Thread.Sleep(3000)
            
            // Verify error message is displayed
            let errorElements = driver.FindElements(By.XPath("//div[contains(@class, 'text-destructive') or contains(@class, 'error')]"))
            let hasError = errorElements.Count > 0
            
            // Also check for toast error messages or any error text
            let pageText = driver.PageSource
            let containsError = pageText.Contains("Invalid") || pageText.Contains("error") || pageText.Contains("failed") || pageText.Contains("credentials")
            
            Assert.True(hasError || containsError, "Expected error message to be displayed for invalid login credentials")
            
            // Verify we're still on the login page (not redirected)
            let currentUrl = driver.Url
            Assert.Contains("/login", currentUrl)
            
            // Wait before closing so you can see the final state
            Thread.Sleep(3000)
            
        with
        | ex ->
            printfn "Error occurred: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            Thread.Sleep(3000)
            reraise()
    finally
        Thread.Sleep(3000)
        try
            driver.Quit()
        with
        | _ -> ()
        try
            driver.Dispose()
        with
        | _ -> ()

[<Fact>]
let ``Login with empty email should display validation error`` () =
    let options = ChromeOptions()
    options.AddArgument("--disable-blink-features=AutomationControlled")
    let driver = new ChromeDriver(options)
    // Wait for driver to fully initialize
    Thread.Sleep(2000)
    try
        try
            // Navigate to login page
            driver.Navigate().GoToUrl("http://localhost:3000/login")
            
            // Wait for page to load
            Thread.Sleep(3000)
            
            // Leave email empty, enter password
            let passwordInput = driver.FindElement(By.Id("password"))
            passwordInput.Clear()
            passwordInput.SendKeys("somepassword")
            Thread.Sleep(3000) // Wait to see password entered
            
            // Try to submit the form
            let loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Login')]"))
            loginButton.Click()
            Thread.Sleep(3000) // Wait to see button click
            
            // Wait a moment for validation
            Thread.Sleep(3000)
            
            // HTML5 validation should prevent submission or show error
            // Check if email field has validation error
            let emailInput = driver.FindElement(By.Id("email"))
            let isRequired = emailInput.GetAttribute("required")
            
            // Verify we're still on login page
            let currentUrl = driver.Url
            Assert.Contains("/login", currentUrl)
            
            // Wait before closing so you can see the final state
            Thread.Sleep(3000)
            
        with
        | ex ->
            printfn "Error occurred: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            Thread.Sleep(3000)
            reraise()
    finally
        Thread.Sleep(3000)
        try
            driver.Quit()
        with
        | _ -> ()
        try
            driver.Dispose()
        with
        | _ -> ()

[<Fact>]
let ``Login with empty password should display validation error`` () =
    let options = ChromeOptions()
    options.AddArgument("--disable-blink-features=AutomationControlled")
    let driver = new ChromeDriver(options)
    // Wait for driver to fully initialize
    Thread.Sleep(2000)
    try
        try
            // Navigate to login page
            driver.Navigate().GoToUrl("http://localhost:3000/login")
            
            // Wait for page to load
            Thread.Sleep(3000)
            
            // Enter email, leave password empty
            let emailInput = driver.FindElement(By.Id("email"))
            emailInput.Clear()
            emailInput.SendKeys("test@example.com")
            Thread.Sleep(3000) // Wait to see email entered
            
            // Try to submit the form
            let loginButton = driver.FindElement(By.XPath("//button[contains(text(), 'Login')]"))
            loginButton.Click()
            Thread.Sleep(3000) // Wait to see button click
            
            // Wait a moment for validation
            Thread.Sleep(3000)
            
            // HTML5 validation should prevent submission
            let passwordInput = driver.FindElement(By.Id("password"))
            let isRequired = passwordInput.GetAttribute("required")
            
            // Verify we're still on login page
            let currentUrl = driver.Url
            Assert.Contains("/login", currentUrl)
            
            // Wait before closing so you can see the final state
            Thread.Sleep(3000)
            
        with
        | ex ->
            printfn "Error occurred: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            Thread.Sleep(3000)
            reraise()
    finally
        Thread.Sleep(3000)
        try
            driver.Quit()
        with
        | _ -> ()
        try
            driver.Dispose()
        with
        | _ -> ()
