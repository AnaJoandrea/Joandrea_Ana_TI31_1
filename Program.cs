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

                string caleaCatreStopwords = @"D:\Desktop\Facultate\Anul 3\L Sisteme de recunoastere a formelor\stopwords.txt";

                if (!File.Exists(caleaCatreStopwords))
                {
                    Console.WriteLine("EROARE: Nu gasesc fisierul stopwords.txt la calea: " + caleaCatreStopwords);
                    Console.WriteLine("Apasa orice tasta pentru a iesi...");
                    Console.ReadKey();
                    return;
                }

                FeatureExtractor extractor = new FeatureExtractor(caleaCatreStopwords);
                string folderTraining = @"D:\Desktop\Facultate\Anul 3\L Sisteme de recunoastere a formelor\Reuters\Reuters_34\Training";
                string[] fisiereXml = Directory.GetFiles(folderTraining, "*.XML");

                Console.WriteLine($"Am gasit {fisiereXml.Length} articole XML in folderul Training. Procesam...");

                if (fisiereXml.Length == 0)
                {
                    Console.WriteLine("EROARE: Nu s-au gasit fisiere XML in folderul Training.");
                    Console.WriteLine("Apasa orice tasta pentru a iesi...");
                    Console.ReadKey();
                    return;
                }

                string fisierRezultat = @"D:\Desktop\Facultate\Anul 3\L Sisteme de recunoastere a formelor\rezultat_vectori_training.txt";

                extractor.ProcessDocuments(fisiereXml, fisierRezultat, "Training");

                extractor.SaveGlobalDictionary(@"D:\Desktop\Facultate\Anul 3\L Sisteme de recunoastere a formelor\dictionar_global.txt");

                Console.WriteLine("Extragerea s-a finalizat cu succes! Verifica fisierul rezultat_vectori_training.txt.");
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