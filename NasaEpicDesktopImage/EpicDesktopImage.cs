using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace NasaEpicDesktopImage
{
    public class EpicDesktopImage
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public async Task SetDesktopImageAsync(string nasaApiKey)
        {
            await GetLatestEpicImageAsync(nasaApiKey);
        }

        private async Task GetLatestEpicImageAsync(string nasaApiKey)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync($"https://api.nasa.gov/EPIC/api/natural/images?api_key={nasaApiKey}");
                if (result.IsSuccessStatusCode)
                {
                    var jsonString = await result.Content.ReadAsStringAsync();
                    EpicModel[] epicModel = JsonSerializer.Deserialize<EpicModel[]>(jsonString);
                    if (epicModel.Length > 0)
                    {
                        var latestRecord = epicModel.Last();
                        var parsedDate = DateTime.Parse(latestRecord.Date);
                        string formattedDate = parsedDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                        string ImageUrl = $"https://api.nasa.gov/EPIC/archive/natural/{formattedDate}/png/{latestRecord.Image}.png?api_key={nasaApiKey}";
                        SetDesktopImage(new Uri(ImageUrl), Style.Centered, latestRecord);
                    }
                }
            }
        }

        private void AddTextToEpicImage(Image image, EpicModel epicModel)
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                using (Font arialFont = new Font("Arial", 32))
                {
                    graphics.DrawString(epicModel.Date, arialFont, Brushes.White, 830, 1900);
                }
            }
        }

        private void SetDesktopImage(Uri uri, Style style, EpicModel epicModel)
        {
            Stream s = new System.Net.WebClient().OpenRead(uri.ToString());
            Image img = Image.FromStream(s);
            AddTextToEpicImage(img, epicModel);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }
    }
}