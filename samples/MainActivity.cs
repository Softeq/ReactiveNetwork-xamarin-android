using System.Threading;
using Android.Content;
using Android.Net;
using Android.OS;
using Com.Github.Pwittchen.Reactivenetwork.Library.Rx2;
using Com.Github.Pwittchen.Reactivenetwork.Library.Rx2.Internet.Observing;
using Java.Interop;
using Java.Lang;
using Org.Apache.Http;
using ReactiveX;
using ReactiveX.Android.Schedulers;
using ReactiveX.Functions;
using ReactiveX.Internal.Observers;
using ReactiveX.Schedulers;

namespace ReactiveNetworkSamples;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    private ReactiveX.Disposables.IDisposable? _observable;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);


        _observable?.Dispose();


        var settings = InternetObservingSettings.InvokeBuilder()
            .InitialInterval(1000)
            .Interval(1000)
                .Host("https://google.com")
            .Timeout(5000)
            .HttpResponse(200)
            //.ErrorHandler(testErrorHandler)
            //.Strategy(strategy)
            .Build();

        _observable = ReactiveNetwork
            //.ObserveInternetConnectivity(settings)
            .ObserveNetworkConnectivity(this)
            .SubscribeOn(Schedulers.Io())!
            // ... anything else what you can do with RxJava
            .ObserveOn(AndroidSchedulers.MainThread())!
            .Subscribe(
                onNext: new WeakConsumer<Connectivity>((connectivity) => /* handle connectivity here */
                {
                    // do something with connectivity
                    var state = connectivity.State();
                    var type = connectivity.Type();
                }),
                onError: new WeakConsumer<Throwable>((throwable) => /* handle error here */
                {

                }));
    }
}


public class WeakConsumer<T> : Java.Lang.Object, IConsumer
{
    private readonly WeakReference<Action<T>> _callbackWeak;

    public WeakConsumer(Action<T> callback)
    {
        _callbackWeak = new WeakReference<Action<T>>(callback);
    }

    public void Accept(Java.Lang.Object? t)
    {
        if (_callbackWeak.TryGetTarget(out var target) && t is T arg)
        {
            target(arg);
        }
        else
        {
            // TODO: log
        }
    }
}