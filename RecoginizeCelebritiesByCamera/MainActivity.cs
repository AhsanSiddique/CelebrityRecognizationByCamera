using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Graphics;
using System.Net.Http;
using System.IO;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android;
using Android.Provider;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;

namespace RecoginizeCelebrities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const string subscriptionKey = "3407ad6140b240f58847194ebf0dc26d";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/analyze";
        ImageView imageView;
        Bitmap mBitMap;
        int CAMERA_CODE = 1000, CAMERA_REQUEST = 1001;
        ByteArrayContent content;
        TextView txtDes;
        Button btnProcess, btnCapture;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == CAMERA_CODE)
            {
                if (grantResults[0] == Permission.Granted)
                    Toast.MakeText(this, "Permission Granted", ToastLength.Short).Show();
                else
                    Toast.MakeText(this, "Permission Not Granted", ToastLength.Short).Show();
            }
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 0 && resultCode == Android.App.Result.Ok &&
                data != null)
            {
                mBitMap = (Bitmap)data.Extras.Get("data");
                imageView.SetImageBitmap(mBitMap);
                byte[] bitmapData;
                using (var stream = new MemoryStream())
                {
                    mBitMap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    bitmapData = stream.ToArray();
                }
                content = new ByteArrayContent(bitmapData);
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //Request runtime permission
            if (CheckSelfPermission(Manifest.Permission.Camera) == Android.Content.PM.Permission.Denied)
            {
                RequestPermissions(new string[] { Manifest.Permission.Camera }, CAMERA_REQUEST);
            }
            txtDes = FindViewById<TextView>(Resource.Id.txtDescription);
            imageView = FindViewById<ImageView>(Resource.Id.image);
            btnProcess = FindViewById<Button>(Resource.Id.btnProcess);
            btnCapture = FindViewById<Button>(Resource.Id.btnCapture);

            btnCapture.Click += delegate
            {
                Intent intent = new Intent(MediaStore.ActionImageCapture);
                StartActivityForResult(intent, 0);
            };

            btnProcess.Click += async delegate
            {
                await MakeAnalysisRequest(content);
            };
        }
        public async Task MakeAnalysisRequest(ByteArrayContent content)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters =
                    "visualFeatures=Categories&details=Celebrities";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                var response = await client.PostAsync(uri, content);

                // Asynchronously get the JSON response.


                string contentString = await response.Content.ReadAsStringAsync();

                var analysesResult = JsonConvert.DeserializeObject<AnalysisModel>(contentString);

                txtDes.Text = "Name: " + analysesResult.categories[0].detail.celebrities[0].name.ToString();
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "" + e.ToString(), ToastLength.Short).Show();
            }
        }
    }
}

