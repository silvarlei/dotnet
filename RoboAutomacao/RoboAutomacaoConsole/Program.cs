// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Linq;
using RoboAutomacaoConsole;
using System;
using OpenQA.Selenium.DevTools.V111.Network;

Console.WriteLine("Hello, World!");


API api = new API();


var url = await api.Authorize();


// Chrome Driver was manually downloaded from https://sites.google.com/a/chromium.org/chromedriver/downloads
// parameter "." will instruct to look for the chromedriver.exe in the current folder
using (var driver = new ChromeDriver("."))
{
    //Navigate to DotNet website
    driver.Navigate().GoToUrl(url);
    //Click the Get Started button
    driver.FindElement(By.Id("EmailCpf")).Click();

    // Get Started section is a multi-step wizard
    // The following sections will find the visible next step button until there's no next step button left

    var elementText = driver.FindElement(By.Id("mat-input-0"));
    elementText.SendKeys("administrator");

    var elementSenha = driver.FindElement(By.Id("mat-input-1"));
    elementSenha.SendKeys("Pa$$word123");



    // driver.FindElement(By.ClassName("ng-star-inserted")).Click();

    driver.FindElement(By.XPath("//button-localiza")).Click();


    Thread.Sleep(10000);

    IWebElement nextLink = null;


    var urlAtual = driver.Url;
    string paran = "";
    var code = api.ParseQuery(urlAtual);
    foreach (var element in code)
    {
         paran = element.Value;
        break;
    }


    var _auth = await api.GetTokens(null, paran);

   
    //var urlAll = await api.SendUserInfo(_auth.AccessToken);

    new Metodos().Test2(_auth.RefreshToken);

    //do
    //{
    //    nextLink?.Click();
    //    nextLink = driver.FindElements(By.CssSelector(".step:not([style='display:none;']):not([style='display: none;']) .step-select")).FirstOrDefault();
    //} while (nextLink != null);

    // on the last step, grab the title of the step wich should be equal to "Next steps"
    //var lastStepTitle = driver.FindElement(By.CssSelector(".step:not([style='display:none;']):not([style='display: none;']) h2")).Text;

    // verify the title is the expected value "Next steps"
    //Assert.AreEqual(lastStepTitle, "Next steps");
}


