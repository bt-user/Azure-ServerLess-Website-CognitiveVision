#r "Newtonsoft.Json"
using ImageResizer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

static HttpClient httpClient = new HttpClient();

        private static readonly List<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Tags
        };

public static async Task<object> Run(Stream testblob, string name, Stream thumbnail, TraceWriter log)
{
    var instructions = new Instructions
    {
        Width = 200,
        Height = 200,
        Mode = FitMode.Crop,
        Scale = ScaleMode.Both
    };
    
    ImageBuilder.Current.Build(new ImageJob(testblob, thumbnail, instructions){
                                        DisposeSourceObject = false,
                                        ResetSourceStream = true
                                    });

ComputerVisionClient computerVision = new ComputerVisionClient
(new ApiKeyServiceClientCredentials(System.Environment.GetEnvironmentVariable("COMP_VISION_KEY", EnvironmentVariableTarget.Process)));
computerVision.Endpoint = System.Environment.GetEnvironmentVariable("COMP_VISION_URL", EnvironmentVariableTarget.Process);

ImageAnalysis analysis = await computerVision.AnalyzeImageInStreamAsync(
                    testblob, features);
log.Info("Found This:" + analysis.Description.Captions[0].Text);
return new {
        id = name,
        imgPath = "/images/" + name,
        thumbnailPath = "/thumbnails/" + name,
        description = analysis.Description
    };
        
//OCR Start//
const bool DetectOrientation = true;
Log("Calling ComputerVisionClient.RecognizePrintedTextInStreamAsync()...");
OcrResult ocrResult = await computerVision.RecognizePrintedTextInStreamAsync(!DetectOrientation, testblob, OcrLanguages.En);
    return new {
        id = name,
        imgPath = "/images/" + name,
        thumbnailPath = "/thumbnails/" + name,
        description = ocrResult.Description
    };
//OCR End//
}
