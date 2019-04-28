using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace Hackathon
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            Button send = new Button
            {
                Text = "send",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = Color.Accent,
                TextColor = Color.White,
                CornerRadius = 25,
            };
            int sum = 0;
            Label sumText = new Label();
            sumText.Text = $" Sum: {sum.ToString()} rub.\n";
            Label s = new Label();
            this.Content = s;
            Xamarin.Forms.Image img = new Xamarin.Forms.Image();
            MediaFile photo;
            var cls = new Classifier();

            send.Clicked += async (o, e) =>
            {
                if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
                {
                    MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        SaveToAlbum = true,
                        Directory = "Scanned",
                        Name = $"{DateTime.Now.ToString("Scan_dd.MM.yyyy_hh.mm.ss")}.jpg"
                    });
                    if (file == null) return;
                    img.Source = ImageSource.FromFile(file.Path);
                    photo = file;
                    var filesteam = File.OpenRead(photo.Path);
                    var res = cls.Predict(filesteam);
                    foreach (var c in res)
                    {
                        if(c.Probability > 0.8)
                        {
                            s.Text += $"{c.TagName}";
                            if (c.TagName == "lays")
                            {
                                s.Text += $" - {30} rub.\n";
                                sum += 30;
                            }
                            else if (c.TagName == "cookie")
                            {
                                s.Text += $" - {15} rub.\n";
                                sum += 15;
                            }
                            else if (c.TagName == "fanta")
                            {
                                s.Text += $" - {46} rub.\n";
                                sum += 46;
                            }
                        }
                    }
                    sumText.Text = $" Sum: {sum.ToString()} rub.\n";
                }
            };
            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children = {
                    img,
                    new StackLayout
                    {
                         Children = {send},
                         Orientation =StackOrientation.Horizontal,
                         HorizontalOptions = LayoutOptions.CenterAndExpand
                    },
                    sumText,
                    s,
                }
            };
            ScrollView scrollView = new ScrollView();
            scrollView.Content = Content;
            this.Content = scrollView;
        }
    }
    class Classifier
    {
        private string trainingKey = "{Training-Key}";
        private string predictionKey = "{Prediction-Key}";
        private string endpointUrl = "{Prediction-URL}";

        private CustomVisionTrainingClient trainingApi;
        private CustomVisionPredictionClient endpoint;
        private Project project;

        public Classifier()
        {
            trainingApi = new CustomVisionTrainingClient()
            {
                ApiKey = trainingKey,
                Endpoint = endpointUrl
            };
            endpoint = new CustomVisionPredictionClient()
            {
                ApiKey = predictionKey,
                Endpoint = endpointUrl
            };
            var projects = trainingApi.GetProjects();
            project = projects.FirstOrDefault(p => p.Name == "objDetection");
        }

        public List<PredictionModel> Predict(Stream image)
        {
            var res = endpoint.DetectImage(project.Id, "Iteration1", image);
            var pred = new List<PredictionModel>(res.Predictions);
            return pred;
        }
    }
}