using Discord.Interactions;
using Discord.WebSocket;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obscura.Commands.Google
{
    internal class imageSearch
    {
        private static IWebDriver driver = new FirefoxDriver();

        public static async Task RequestImageSearch(string search, SocketInteraction context)
        {
            driver.Manage().Window.Maximize();
            //This will use selenium to scrape google images for the top 10 results for the search term
            driver.Navigate().GoToUrl($"https://www.google.com/search?q={search}");
            Task.Delay(1000).Wait();
            driver.FindElement(By.XPath("//*[@id=\"L2AGLb\"]")).Click();
            Task.Delay(250).Wait();
            driver.FindElement(By.XPath("/html/body/div[4]/div/div[4]/div/div[1]/div/div/div[1]/div/div/div/div/div[1]/div/div[2]/a")).Click();
            Task.Delay(250).Wait();
            var mainFlex = driver.FindElement(By.XPath("/html/body/div[3]/div/div[15]/div/div[2]/div[2]/div/div/div/div"));
            var image1 = mainFlex.FindElements(By.ClassName("YQ4gaf")).First().GetAttribute("src");
            await context.Channel.SendMessageAsync(image1);
        }
    }
}
