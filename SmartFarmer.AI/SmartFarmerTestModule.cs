using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.AI;

public class SmartFarmerTestModule : ISmartFarmerAIPlantDetector
{
    public string PlantId => null;
    public string PlantBotanicalName => "PlantBotanicalName_1";

    public async Task<FarmerAIDetectionLog> ExecuteDetection(object stepData)
    {
        FarmerAIDetectionLog log = new FarmerAIDetectionLog();

        await Task.CompletedTask;
        SmartFarmerLog.Information($"processing {stepData}");

        Process(stepData.ToString());

        return log;
    }

    private void Process(string filename)
    {
        var image = new Image<Bgr, byte>(filename);
        var grayImage = image.Convert<Gray, byte>();

        SavePictureToDisk(grayImage.ToBitmap(), filename);
    }

    private string SavePictureToDisk(Bitmap image, string filename)
    {
        // Get an ImageCodecInfo object that represents the JPEG codec.
        var imageCodecInfo = GetEncoderInfo("image/jpeg");
                     
        // Create an Encoder object based on the GUID
                     
        // for the Quality parameter category.
        var encoder = Encoder.Quality;

        var encoderParameters = new EncoderParameters(1);

        // Save the bitmap as a JPEG file with quality level 100.
        var encoderParameter = new EncoderParameter(encoder, 100L);
        
        encoderParameters.Param[0] = encoderParameter;

        var lastIndex = filename.LastIndexOf(".");
        var extension = filename.Substring(lastIndex);

        var imageFilename = filename.Substring(0, lastIndex) + "_converted" + extension;

        image.Save(imageFilename, imageCodecInfo, encoderParameters);

        return imageFilename;
    }

    private static ImageCodecInfo GetEncoderInfo(String mimeType)
    {
        int j;
        ImageCodecInfo[] encoders;
        encoders = ImageCodecInfo.GetImageEncoders();
        for(j = 0; j < encoders.Length; ++j)
        {
            if(encoders[j].MimeType == mimeType)
                return encoders[j];
        }
        return null;
    }
}