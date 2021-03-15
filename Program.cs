using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace JumiaWebScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string webUrl = "https://www.jumia.com.tn";
            HtmlWeb web = new HtmlWeb();

            HtmlDocument doc = web.Load(webUrl+ "/peripheriques-logiciels-accessoires/");

            List<string> hrefArticles = new List<string>();

            foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
            {
                hrefArticles.Add(item.Attributes["href"].Value);
            }

            var nbArticlesText = doc.DocumentNode.SelectNodes("//p [@class=\"-gy5 -phs\"]");
            
            int nbArticles = Int32.Parse(nbArticlesText[0].InnerText.Substring(0, nbArticlesText[0].InnerText.IndexOf(" ")));

            int nbPages = nbArticles / 40;
            if (nbPages > 50) nbPages = 50;

            for (int i = 0; i < nbPages-1; i++)
            {
                var nextPage = doc.DocumentNode.SelectNodes("//a[@aria-label=\"Page suivante\"]").ToList();

                //Console.WriteLine(nextPage[0].Attributes["href"].Value);
                Console.WriteLine("charging page "+(i+2).ToString()+" / "+nbPages.ToString());

                string link = webUrl + nextPage[0].Attributes["href"].Value;
                doc = web.Load(link);
                foreach (var item in doc.DocumentNode.SelectNodes("//*[@class=\"core\"]/@href"))
                {
                    hrefArticles.Add(item.Attributes["href"].Value);
                }
            }
            
            List<string> nomArticles = new List<string>();
            List<string> prixArticles = new List<string>();
            List<string> skuArticles = new List<string>();
            List<string> couleurArticles = new List<string>();
            List<string> modelArticles = new List<string>();
            List<string> poidsArticles = new List<string>();
            List<string> finalArticles = new List<string>();
            decimal prog = 0;

            for (int i = 0; i < /*hrefArticles.Count -*/10; i++)
            {
                prog = i / 10;
                Console.WriteLine($"{prog/10}");
                string articleLink = webUrl + hrefArticles[i];
                doc = web.Load(articleLink);
                foreach (var item in doc.DocumentNode.SelectNodes("//h1/@class"))
                {
                    nomArticles.Add(item.InnerText);
                    break;
                }
                foreach (var item in doc.DocumentNode.SelectNodes("//span[@dir=\"ltr\"]"))
                {
                    prixArticles.Add(item.InnerText.Substring(0, item.InnerText.IndexOf(" ")));
                    break;
                }
                foreach (var item in doc.DocumentNode.SelectNodes("//article[3]/div/ul/li[1]/text()"))
                {
                    skuArticles.Add(item.InnerText.Substring(2));
                    break;
                }
                foreach (var item in doc.DocumentNode.SelectNodes("//article[3]/div/ul/li[2]/text()"))
                {
                    couleurArticles.Add(item.InnerText.Substring(2));
                    break;
                }
                foreach (var item in doc.DocumentNode.SelectNodes("//article[3]/div/ul/li[3]/text()"))
                {
                    modelArticles.Add(item.InnerText.Substring(2));
                    break;
                }
                foreach (var item in doc.DocumentNode.SelectNodes("//article[3]/div/ul/li[4]/text()"))
                {
                    poidsArticles.Add(item.InnerText.Substring(2));
                    break;
                }

                finalArticles.Add(string.Concat(nomArticles[i],";",prixArticles[i], ";", skuArticles[i], ";", couleurArticles[i], ";", modelArticles[i], ";", poidsArticles[i]));
            }

            foreach (string article in finalArticles)
                Console.WriteLine(article);

            Console.WriteLine(hrefArticles.Count+"  articles");
            Console.WriteLine(nbArticles + "  articles");
            
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
 
