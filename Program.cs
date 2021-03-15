using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JumiaWebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string webUrl = "https://www.jumia.com.tn";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(webUrl + "/son-informatique/");
            
            //extract href articles 1 ere page
            List<string> hrefArticles = new List<string>();
            foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
                hrefArticles.Add(item.Attributes["href"].Value);

            //nombre de pages à parcourir
            var nbArticlesText = doc.DocumentNode.SelectSingleNode("//p [@class=\"-gy5 -phs\"]");
            int nbArticles = Int32.Parse(nbArticlesText.InnerText.Substring(0, nbArticlesText.InnerText.IndexOf(" ")));
            int nbPages = (int)(((nbArticles / 40) > 50) ? 50 : Math.Round(Convert.ToDecimal(nbArticles / 40))+1);

            for (int i = 0; i < nbPages - 1; i++)
            {
                Console.WriteLine($"charging page{i + 2}/ {nbPages}");
                string link = webUrl + doc.DocumentNode.SelectSingleNode("//a[@aria-label=\"Page suivante\"]").Attributes["href"].Value;
                doc = web.Load(link);
                foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
                {
                    hrefArticles.Add(item.Attributes["href"].Value);
                }
            }

            //Parcourir tous les articles
            List<string> finalArticles = new List<string> { "Nom;Prix;SKU;Couleur;Model;Poids" };
            for (int i = 0; i < hrefArticles.Count -1; i++)
            {
                Console.WriteLine($"Progress {Math.Round((Convert.ToDecimal(i) / hrefArticles.Count)*100,2)} %");
                string articleLink = webUrl + hrefArticles[i];
                doc = web.Load(articleLink);

                var nom = doc.DocumentNode.SelectSingleNode("//h1/@class")?.InnerText;
                var prix = doc.DocumentNode.SelectSingleNode("//span[@dir=\"ltr\"]")?.InnerText;
                var sku = doc.DocumentNode.SelectSingleNode("//*[text()=\"SKU\"]//following-sibling::text()")?.InnerText.Substring(2);
                var couleur = doc.DocumentNode.SelectSingleNode("//*[text()=\"Couleur\"]//following-sibling::text()")?.InnerText.Substring(2);
                var modele = doc.DocumentNode.SelectSingleNode("//*[text()=\"Modèle\"]//following-sibling::text()")?.InnerText.Substring(2);
                var poids = doc.DocumentNode.SelectSingleNode("//*[text()=\"Poids (kg)\"]//following-sibling::text()")?.InnerText.Substring(2);

                finalArticles.Add(string.Concat(nom, ",", prix, ",", sku, ",", couleur, ",", modele, ",", poids));
            }

            foreach (string article in finalArticles)
                Console.WriteLine(article);

            Console.WriteLine(hrefArticles.Count + "  articles");

            //creation fichier csv
            StringBuilder sb = new StringBuilder();
            foreach (var article in finalArticles)
            {
                sb.AppendLine(article);
            }
            File.WriteAllText(@"C:\Mon Travail\Articles.csv", sb.ToString());

            Console.WriteLine("Download complete successfully!!!");
        }
    }
}
