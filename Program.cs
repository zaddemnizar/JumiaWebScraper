using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace jumiascraping
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            string webUrl = "https://www.jumia.com.tn";
            string webCode = client.DownloadString(webUrl + "/son-informatique/");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webCode);
            
            //extract liens articles 1ere page
            List<string> hrefArticles = new List<string>();
            foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
                hrefArticles.Add(item.Attributes["href"].Value);
            
            //nombre de pages à parcourir
            var nbArticlesText = doc.DocumentNode.SelectSingleNode("//p [@class=\"-gy5 -phs\"]");
            int nbArticles = Int32.Parse(nbArticlesText.InnerText.Substring(0, nbArticlesText.InnerText.IndexOf(" ")));
            int nbPages = (int)(((nbArticles / 40) > 50) ? 50 : Math.Round(Convert.ToDecimal(nbArticles / 40)) + 1);

            for (int i = 0; i < nbPages - 1; i++)
            {
                Console.WriteLine($"charging page {i + 2}/ {nbPages}");
                string link = client.DownloadString(webUrl + doc.DocumentNode.SelectSingleNode("//a[@aria-label=\"Page suivante\"]").Attributes["href"].Value);
                doc.LoadHtml(link);
                foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
                {
                    hrefArticles.Add(item.Attributes["href"].Value);
                }
            }

            //Parcourir tous les articles
            List<string> finalArticles = new List<string> { "Nom;Prix;SKU;Couleur;Model;Poids" };
            for (int i = 0; i < hrefArticles.Count - 1; i++)
            {
                Console.WriteLine($"Progress {Math.Round((Convert.ToDecimal(i) / hrefArticles.Count) * 100, 2)} %");
                string articleLink = client.DownloadString(webUrl + hrefArticles[i]);
                doc.LoadHtml(articleLink);

                var nom = doc.DocumentNode.SelectSingleNode("//h1/@class")?.InnerText;
                var prix = doc.DocumentNode.SelectSingleNode("//span[@dir=\"ltr\"]")?.InnerText.Replace(";", " ");
                var sku = doc.DocumentNode.SelectSingleNode("//*[text()=\"SKU\"]//following-sibling::text()")?.InnerText.Substring(2);
                var couleur = doc.DocumentNode.SelectSingleNode("//*[text()=\"Couleur\"]//following-sibling::text()")?.InnerText.Substring(2);
                var modele = doc.DocumentNode.SelectSingleNode("//*[text()=\"Modèle\"]//following-sibling::text()")?.InnerText.Substring(2);
                var poids = doc.DocumentNode.SelectSingleNode("//*[text()=\"Poids (kg)\"]//following-sibling::text()")?.InnerText.Substring(2);

                //télécharger image correspondante
                string imageurl = doc.DocumentNode.SelectSingleNode("//img[@class=\"-fw -fh\"]").Attributes["data-src"].Value;
                client.DownloadFile(imageurl, sku+ ".jpg");

                finalArticles.Add(string.Concat(nom, ";", prix, ";", sku, ";", couleur, ";", modele, ";", poids));
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
