using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;

namespace WebApplication3
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        public class ValorUf
        {
            public string MesStr { get; set; }
            public string Uf { get; set; }
            public string DiaStr { get; set; }
            public DateTime Fecha { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var ufs = ScrappingStatic("2023");
            Grilla.DataSource = ufs;
            Grilla.DataBind();
            //    var epis=  ScrappingDinamic();
        }


        private static List<ValorUf> ScrappingStatic(string anio)
        {
      
            var url = "https://www.sii.cl/valores_y_fechas/uf/uf" + anio + ".htm";

            var web = new HtmlWeb();
            var document = web.Load(url);

            var tablas = document.DocumentNode.SelectNodes("//div[contains(@id,'mes')]//table[not(contains(@id,'export'))]//tbody");

            var episodes = new List<ValorUf>();
            var mesNom = "";
            var mesNum = 0;


            foreach (HtmlNode tabla in tablas.ToList())
            {

                foreach (HtmlNode fila in tabla.SelectNodes("tr"))
                {
                    var diaNom = "";
                    var uf = "";
                    var fecha = DateTime.Today;

                    foreach (HtmlNode cell in fila.SelectNodes("th|td"))
                    {
                        var tipoLinea = cell.InnerHtml.Contains("h2") ? "mes" : "dia";

                        if (tipoLinea == "mes")
                        {
                            mesNom = cell.InnerText;
                            mesNum = DateTime.ParseExact(mesNom, "MMMM", CultureInfo.CurrentCulture).Month;
                        }
                        else
                        {
                            if (cell.InnerText == "") continue;

                            if (cell.Name != "th")
                            {
                                uf = cell.InnerText;
                            }
                            else
                            {
                                diaNom = cell.InnerText;
                                var fechaStr = diaNom + "-" + mesNum + "-" + anio;
                                DateTime.TryParse(fechaStr, out fecha);
                            }

                            if (uf == "") continue;
                            episodes.Add(new ValorUf
                            {
                                MesStr = mesNom,
                                DiaStr = diaNom,
                                Uf = uf,
                                Fecha = fecha
                            });

                            uf = "";

                        }
                    }
                }
            }



            return episodes.OrderBy(x => x.Fecha).ToList();
        }


        public static List<ValorUf> ScrappingDinamic()
        {
            // the URL of the target Wikipedia page
            var url = "https://en.wikipedia.org/wiki/List_of_SpongeBob_SquarePants_episodes";

            // to initialize the Chrome Web Driver in headless mode
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            var driver = new ChromeDriver();
            // connecting to the target web page
            driver.Navigate().GoToUrl(url);

            // selecting the HTML nodes of interest 
            var nodes = driver.FindElements(By.XPath("//*[@id='mw-content-text']/div[1]/table[position()>1 and position()<15]/tbody/tr[position()>1]"));

            // initializing the list of objects that will
            // store the scraped data
            List<ValorUf> episodes = new();
            // looping over the nodes 
            // and extract data from them
            foreach (var node in nodes)
            {
                // add a new Episode instance to 
                // to the list of scraped data
                episodes.Add(new ValorUf()
                {
                    MesStr = node.FindElement(By.XPath("th[1]")).Text,
                    Uf = node.FindElement(By.XPath("td[2]")).Text,
                    DiaStr = node.FindElement(By.XPath("td[3]")).Text,
                    //   WrittenBy = node.FindElement(By.XPath("td[4]")).Text,
                    //Released = node.FindElement(By.XPath("td[5]")).Text
                });
            }

            return episodes;
            // converting the scraped data to CSV... 
            // storing this data in a db...
            // calling an API with this data...

        }
    }
}