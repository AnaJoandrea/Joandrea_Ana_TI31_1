using System;
using System.IO;

namespace WordFrequency
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Incepem extragerea trasaturilor...");

                string folderBaza = @"D:\Desktop\Facultate\Anul 3\L Sisteme de recunoastere a formelor";
                string caleaCatreStopwords = Path.Combine(folderBaza, "stopwords.txt");

                if (!File.Exists(caleaCatreStopwords))
                {
                    Console.WriteLine("EROARE: Nu gasesc fisierul stopwords.txt la calea: " + caleaCatreStopwords);
                    Console.ReadKey();
                    return;
                }

                FeatureExtractor extractor = new FeatureExtractor(caleaCatreStopwords);

                string folderReuters = Path.Combine(folderBaza, @"Reuters\Reuters_34");
                string folderTraining = Path.Combine(folderReuters, "Training");
                string folderTesting = Path.Combine(folderReuters, "Testing");
                if (!Directory.Exists(folderTesting))
                {
                    folderTesting = Path.Combine(folderReuters, "Test");
                }

                if (Directory.Exists(folderTraining))
                {
                    string[] fisiereTraining = Directory.GetFiles(folderTraining, "*.XML");
                    Console.WriteLine($"Am gasit {fisiereTraining.Length} articole XML in Training. Extragem cuvintele...");
                    extractor.AddDocuments(fisiereTraining, "Training");
                }
                else
                {
                    Console.WriteLine("ATENTIE: Nu am gasit folderul Training la: " + folderTraining);
                }

                if (Directory.Exists(folderTesting))
                {
                    string[] fisiereTesting = Directory.GetFiles(folderTesting, "*.XML");
                    Console.WriteLine($"Am gasit {fisiereTesting.Length} articole XML in Testing. Extragem cuvintele...");
                    extractor.AddDocuments(fisiereTesting, "Testing");
                }
                else
                {
                    Console.WriteLine("ATENTIE: Nu am gasit folderul Testing/Test in: " + folderReuters);
                }

                string fisierRezultat = Path.Combine(folderBaza, "rezultat_vectori.txt");
                extractor.SaveResults(fisierRezultat);

                string fisierDictionar = Path.Combine(folderBaza, "dictionar_global.txt");
                extractor.SaveGlobalDictionary(fisierDictionar);

                Console.WriteLine("Extragerea s-a finalizat cu succes!");
                Console.WriteLine("Verifica fisierul: " + fisierRezultat);
            }
            catch (Exception ex)
            {
                Console.WriteLine("A aparut o eroare: " + ex.Message);
            }

            Console.WriteLine("Apasa orice tasta pentru a inchide programul...");
            Console.ReadKey();
        }
    }
}