using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionDB;
using VisionDB.Controllers;
using VisionDB.Models;

namespace VisionDB.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        public HomeControllerTest()
        {
            LoginViewModel login = new LoginViewModel();
            login.UserName = "a";
            login.Password = "123456";
            login.RememberMe = true;

            AccountController accountController = new AccountController();
            accountController.Login(login, "");
        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Support() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
