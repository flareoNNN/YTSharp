# YTSharp

C# için yazılmış YouTube mp3 indirme kütüphanesi.
Versiyon: *1.0.0*

# Kurulum

Kütüphane youtube-dl desteklidir, kullanmak için `youtube-dl.exe` dosyasına sahip olmalısınız.
youtube-dl Link: [http://ytdl-org.github.io/youtube-dl/download.html](http://ytdl-org.github.io/youtube-dl/download.html)

# Kullanım

````c#
using System;
using YTSharp;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string YTURL = "https://www.youtube.com/watch?v=6i0I-WWpWdc"; // indirilecek dosyanın youtube adresi
            string DosyaIsim = "şarkım"; // mp3 dosyasının adı
            string KayitKonumu = @"C:\Users\user\Desktop"; // mp3 dosyasının kaydedileceği konum
            string BinaryKonumu = @"C:\"; // youtube-dl.exe konumu

            var client = new MP3Client(YTURL, DosyaIsim, KayitKonumu, BinaryKonumu);
            client.Indiriliyor += client_indiriliyor;
            client.Indirildi += client_indirildi;
            client.Indir();

            Console.ReadKey();
        }

        static void client_indiriliyor(object sender, ProgressEventArgs e)
        {
            Console.WriteLine($"Indirme durumu: {e.Percentage}%");
        }

        static void client_indirildi(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Indirme tamamlandi!");
        }
    }
}
````

Çıktı:
````
Indiriliyor: https://www.youtube.com/watch?v=6i0I-WWpWdc
Indirme durumu: 0%
Indirme durumu: 1%
Indirme durumu: 2%
Indirme durumu: 5%
Indirme durumu: 10%
Indirme durumu: 21%
Indirme durumu: 42%
Indirme durumu: 84%
Indirme durumu: 100%
Indirme tamamlandi!
````

# Lisans

Apache License 2.0