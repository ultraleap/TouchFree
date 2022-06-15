using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Microsoft.Extensions.DependencyInjection;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace Ultraleap.TouchFree.Service_Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        public static InteractionManager InteractionManager { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetupServices();
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            // This triggers the TouchFreeService to start
            StartService(new Intent(this, typeof(TouchFreeService)));

            TextView currentCharacterName = FindViewById<TextView>(Resource.Id.textView1);
            currentCharacterName.Text = "LOADED";
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// This sets up the services using MS DI for the TouchFree service
        /// </summary>
        void SetupServices()
        {
            var configFileLocator = new ConfigFileLocator(FilesDir?.Path);
            var services = new ServiceCollection();
            var logger = new TouchFreeLogger();

            services.AddSingleton<IConfigFileLocator>(configFileLocator);
            services.AddSingleton<ITouchFreeLogger>(logger);

            InteractionConfigFile.Logger = logger;
            InteractionConfigFile.ConfigFileLocator = configFileLocator;

            PhysicalConfigFile.Logger = logger;
            PhysicalConfigFile.ConfigFileLocator = configFileLocator;

            services.AddTouchFreeServices();

            ServiceProvider = services.BuildServiceProvider();

            InteractionManager = ServiceProvider.GetService<InteractionManager>();
        }
    }
}