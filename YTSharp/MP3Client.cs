using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace YTSharp
{
    public class MP3Client
    {
        public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);
        public event ProgressEventHandler Indiriliyor;

        public delegate void FinishedDownloadEventHandler(object sender, DownloadEventArgs e);
        public event FinishedDownloadEventHandler Indirildi;

        public delegate void StartedDownloadEventHandler(object sender, DownloadEventArgs e);
        public event StartedDownloadEventHandler IndirmeBasladi;

        public delegate void ErrorEventHandler(object sender, ProgressEventArgs e);
        public event ErrorEventHandler IndirmeHatasi;

        public Object islem { get; set; }
        public bool Basladi { get; set; }
        public bool Bitti { get; set; }
        public decimal Ilerleme { get; set; }
        public Process DLIslem { get; set; }
        public string sonIsim { get; set; }
        public string hedefKlasor { get; set; }
        public string URL { get; set; }
        public string LOG { get; set; }


        public MP3Client(string url, string dosyaIsim, string kayitKonumu, string binaryKonumu)
        {
            this.Basladi = false;
            this.Bitti = false;
            this.Ilerleme = 0;

            hedefKlasor = kayitKonumu;
            URL = url;

            sonIsim = dosyaIsim;
            if (!sonIsim.ToLower().EndsWith(".mp3"))
            {
                sonIsim += ".mp3";
            }

            var binaryYolu = binaryKonumu;
            if (string.IsNullOrEmpty(binaryYolu))
            {
                throw new Exception("youtube-dl konumu bulunamadı/belirtilmedi.");
            }

            var ytdlYOL = Path.Combine(binaryYolu, "youtube-dl.exe");
            if (!File.Exists(ytdlYOL))
            {
                throw new Exception("youtube-dl konumu bulunamadı.");
            }

            var hedefYolu = System.IO.Path.Combine(kayitKonumu, sonIsim);
            if (System.IO.File.Exists(hedefYolu))
            {
                throw new Exception(hedefYolu + " dosyası zaten mevcut.");
            }
            var parametreler = string.Format(@"--continue  --no-overwrites --restrict-filenames --extract-audio --audio-format mp3 {0} -o ""{1}""", url, hedefYolu);

            DLIslem = new Process();
            DLIslem.StartInfo.UseShellExecute = false;
            DLIslem.StartInfo.RedirectStandardOutput = true;
            DLIslem.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            DLIslem.StartInfo.FileName = System.IO.Path.Combine(binaryYolu, "youtube-dl.exe");
            DLIslem.StartInfo.Arguments = parametreler;
            DLIslem.StartInfo.CreateNoWindow = true;
            DLIslem.EnableRaisingEvents = true;

            DLIslem.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            DLIslem.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);
        }

        protected virtual void Islemde(ProgressEventArgs e)
        {
            if (Indiriliyor != null)
            {
                Indiriliyor(this, e);
            }
        }

        protected virtual void IndirmeTamamlandi(DownloadEventArgs e)
        {
            if (Bitti == false)
            {
                Bitti = true;
                Indirildi?.Invoke(this, e);
            }
        }

        protected virtual void IndirmeBaslatti(DownloadEventArgs e)
        {
            IndirmeBasladi?.Invoke(this, e);
        }

        protected virtual void IndirirkenHata(ProgressEventArgs e)
        {
            IndirmeHatasi?.Invoke(this, e);
        }

        public void Indir()
        {
            Console.WriteLine($"Indiriliyor: {URL}");
            DLIslem.Exited += IslemKapatildi;
            DLIslem.Start();
            DLIslem.BeginOutputReadLine();
            this.IndirmeBaslatti(new DownloadEventArgs() { ProcessObject = this.islem });
            while (this.Bitti == false)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        void IslemKapatildi(object sender, EventArgs e)
        {
            IndirmeTamamlandi(new DownloadEventArgs() { ProcessObject = this.islem });
        }

        public void ErrorDataReceived(object sendingprocess, DataReceivedEventArgs error)
        {
            if (!String.IsNullOrEmpty(error.Data))
            {
                this.IndirirkenHata(new ProgressEventArgs() { Error = error.Data, ProcessObject = this.islem });
            }
        }
        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (String.IsNullOrEmpty(outLine.Data) || Bitti)
            {
                return;
            }
            this.LOG += outLine.Data;

            if (outLine.Data.Contains("ERROR"))
            {
                this.IndirirkenHata(new ProgressEventArgs() { Error = outLine.Data, ProcessObject = this.islem });
                return;
            }

            if (!outLine.Data.Contains("[download]"))
            {
                return;
            }
            var pattern = new Regex(@"\b\d+([\.,]\d+)?", RegexOptions.None);
            if (!pattern.IsMatch(outLine.Data))
            {
                return;
            }

            var perc = Convert.ToDecimal(Regex.Match(outLine.Data, @"\b\d+([\.,]\d+)?").Value);
            if (perc > 100 || perc < 0)
            {
                Console.WriteLine($"Beklenmeyen değer (percentage) {perc}");
                return;
            }
            this.Ilerleme = perc;
            this.Islemde(new ProgressEventArgs() { ProcessObject = this.islem, Percentage = perc });

            if (perc < 100)
            {
                return;
            }

            if (perc == 100 && !Bitti)
            {
                IndirmeTamamlandi(new DownloadEventArgs() { ProcessObject = this.islem });
            }
        }
    }
}