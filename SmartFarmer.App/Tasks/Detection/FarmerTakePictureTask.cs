using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using SmartFarmer.Configurations;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Detection;

public class FarmerTakePictureTask : FarmerBaseTask, IFarmerTakePictureTask
{
    private readonly CameraConfiguration _cameraConfiguration;
    private VideoCapture _videoCapture;

    public FarmerTakePictureTask(CameraConfiguration camConfig)
    {
        RequiredTool = FarmerTool.Camera;

        _cameraConfiguration = camConfig;
    }

    public override string TaskName => "Take Picture Task";

    public string FilePath { get; private set; }

    public override void ConfigureTask(IDictionary<string, string> parameters)
    {
        var key = nameof(FilePath);
        
        if (parameters != null && parameters.ContainsKey(key))
        {
            FilePath = parameters[key];
        }
    }

    public async override Task<object> Execute(CancellationToken token)
    {
        await Task.CompletedTask;
        
        VideoCapture capture = GetVideoCapture(); //create a camera capture
        Bitmap image = capture.QueryFrame().ToBitmap(); //take a picture

        SavePictureToDisk(image);

        return FilePath;
    }

    private VideoCapture GetVideoCapture()
    {
        if (_videoCapture == null)
        {
            _videoCapture = new VideoCapture(_cameraConfiguration?.CameraIndex ?? 0);
        }

        return _videoCapture;
    }

    private void SavePictureToDisk(Bitmap image)
    {
        // Get an ImageCodecInfo object that represents the JPEG codec.
        var imageCodecInfo = GetEncoderInfo("image/jpeg");
                     
        // Create an Encoder object based on the GUID
                     
        // for the Quality parameter category.
        var encoder = Encoder.Quality;

        var encoderParameters = new EncoderParameters(1);

        // Save the bitmap as a JPEG file with quality level 75.
        var encoderParameter = new EncoderParameter(encoder, 75L);
        
        encoderParameters.Param[0] = encoderParameter;

        var prefix = _cameraConfiguration?.FilenamePrefix ?? string.Empty;
        var filename = prefix + DateTime.UtcNow + "_" + this.GetType().FullName + ".jpeg";

        FilePath = Path.Combine(new [] { "C:", "Temp", "SmartFarmer", filename});

        image.Save(filename, imageCodecInfo, encoderParameters);
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