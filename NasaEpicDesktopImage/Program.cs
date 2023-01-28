namespace NasaEpicDesktopImage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string nasaApiKey = args[0];
                EpicDesktopImage epicDesktopImage = new EpicDesktopImage();
                epicDesktopImage.SetDesktopImageAsync(nasaApiKey).Wait();
            }
        }
    }
}
