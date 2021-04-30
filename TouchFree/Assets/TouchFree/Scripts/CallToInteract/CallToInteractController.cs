using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
using Ultraleap.ScreenControl.Client.Connection;

public class CallToInteractController : MonoBehaviour
{
    public enum CTIType
    {
        None,
        Image,
        Video
    }

    public static event Action OnCTIActive;
    public static event Action OnCTIInactive;

    public static bool IsShowing { get; private set; }

    public RawImage CTIImage;
    public RawImage CTIVideoImage;
    public VideoPlayer VideoPlayer;
    public float ShowTimeAfterNoHandPresent = 10f;
    public float HideTimeAfterHandPresent = 0.5f;

    public string currentCTIfileName = "";

    public CTIType loadedType;
    RenderTexture videoRenderTexture;
    Stopwatch showTimer = new Stopwatch();
    Stopwatch hideTimer = new Stopwatch();

    HideRequirement hideRequirement = HideRequirement.PRESENT;
    bool interactionHappened = false;

    private static readonly string CTI_PATH = Path.Combine(Application.streamingAssetsPath, "CallToInteract");

    private static readonly string[] CTI_EXTENSIONS = new string[] { ".webm", ".mp4", ".png" };
    private static readonly string[] CTI_VIDEO_EXTENSIONS = new string[] { ".webm", ".mp4" };
    private static readonly string[] CTI_IMAGE_EXTENSIONS = new string[] { ".png" };

    public void UpdateCTISettings(bool _enable, float _showTime, float _hideTime, string _filePath, HideRequirement _hideType)
    {
        _filePath = Path.Combine(CTI_PATH, _filePath);

        ShowTimeAfterNoHandPresent = _showTime;
        HideTimeAfterHandPresent = _hideTime;
        currentCTIfileName = _filePath;            

        HideCTI();
        SetupCTI();

        hideTimer.Reset();
        showTimer.Reset();

        hideRequirement = _hideType;
    }

    public void InteractionHappened()
    {
        interactionHappened = true;
    }

    public void RecreateVideoTexture()
    {
        if (!CallToInteractConfig.Config.Enabled)
        {
            return;
        }

        if (videoRenderTexture != null)
        {
            ReleaseRenderTexture();
        }
        InitVideoPlayer();
    }

    void OnEnable()
    {
        ConnectionManager.HandFound += OnHandEnter;
        ConnectionManager.HandsLost += OnAllHandsExit;

        IsShowing = false;
        SetupCTI();
    }

    void OnDisable()
    {
        ConnectionManager.HandFound -= OnHandEnter;
        ConnectionManager.HandsLost -= OnAllHandsExit;

        if (videoRenderTexture != null)
        {
            ReleaseRenderTexture();
        }
        if (CTIImage.texture != null)
        {
            Destroy(CTIImage.texture);
            CTIImage.texture = null;
        }
    }

    float setupCooldown = 0.5f;
    /// <summary>
    /// This is provided on a cooldown to prevent the loading of video assets on every update of settings values
    /// </summary>
    /// <param name="_afterCooldown"></param>
    void SetupCTI(bool _afterCooldown = false)
    {
        // If the CTI isn't enabled in the config, disable ourselves and exit out.
        if (!CallToInteractConfig.Config.Enabled)
        {
            loadedType = CTIType.None;
            return;
        }

        if(!_afterCooldown)
        {
            setupCooldown = 0.5f;
            return;
        }

        PrepareCTIAsset();

        if (loadedType == CTIType.Video)
        {
            InitVideoPlayer();
        }

        OnAllHandsExit();
    }

    void Update()
    {
        UpdateCTI();

        if(setupCooldown > 0)
        {
            setupCooldown -= Time.deltaTime;

            if(setupCooldown <= 0)
            {
                SetupCTI(true);
            }
        }
    }

    void UpdateCTI()
    {
        if (loadedType != CTIType.None)
        {
            if (!IsShowing && showTimer.Elapsed.TotalSeconds > ShowTimeAfterNoHandPresent && !UIManager.Instance.isActive)
            {
                showTimer.Reset();
                ShowCTI();
            }
            else if (IsShowing)
            {
                if(hideRequirement == HideRequirement.INTERACTION)
                {
                    if(interactionHappened)
                    {
                        HideCTI();
                    }
                }
                
                if (hideRequirement == HideRequirement.PRESENT && hideTimer.Elapsed.TotalSeconds > HideTimeAfterHandPresent)
                {
                    hideTimer.Reset();
                    HideCTI();
                }

                if(UIManager.Instance.isActive)
                {
                    HideCTI();
                    OnAllHandsExit();
                }
            }
        }
        else if (showTimer.IsRunning)
        {
            showTimer.Reset();
        }
    }

    void OnHandEnter()
    {
        showTimer.Reset();

        if (IsShowing && !hideTimer.IsRunning)
        {
            hideTimer.Restart();
        }
    }

    void OnAllHandsExit()
    {
        interactionHappened = false;
        hideTimer.Reset();

        if (!IsShowing && !showTimer.IsRunning)
        {
            showTimer.Restart();
        }
    }

    void ShowCTI()
    {
        IsShowing = true;

        if (loadedType == CTIType.Image)
        {
            CTIImage.enabled = true;
        }
        else if (loadedType == CTIType.Video)
        {
            if (VideoPlayer.isPrepared)
            {
                VideoPlayer_prepareCompleted(VideoPlayer);
            }
            else
            {
                VideoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
                // Wait until the video is prepared again after a call to Stop() before attempting to play to prevent stutter frames showing.
                VideoPlayer.Prepare();
            }
        }

        OnCTIActive?.Invoke();
    }

    void HideCTI()
    {
        IsShowing = false;

        if (loadedType == CTIType.Image)
        {
            CTIImage.enabled = false;
        }
        else if (loadedType == CTIType.Video)
        {
            CTIVideoImage.enabled = false;
            VideoPlayer.Stop();
        }

        OnCTIInactive?.Invoke();
    }

    void VideoPlayer_prepareCompleted(VideoPlayer source)
    {
        VideoPlayer.prepareCompleted -= VideoPlayer_prepareCompleted;
        // Wait until the first frame is ready and rendered before showing the video image to prevent a stutter frame showing.
        VideoPlayer.sendFrameReadyEvents = true;
        VideoPlayer.frameReady += VideoPlayer_frameReady;
        VideoPlayer.Play();
    }

    void VideoPlayer_frameReady(VideoPlayer source, long frameIdx)
    {
        VideoPlayer.sendFrameReadyEvents = false;
        VideoPlayer.frameReady -= VideoPlayer_frameReady;
        // Finally show the video image now that the strutter frames are gone!
        CTIVideoImage.enabled = true;
    }

    void InitVideoPlayer()
    {
        if (videoRenderTexture == null)
        {
            InitRenderTexture();
        }

        VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        VideoPlayer.targetTexture = videoRenderTexture;
        VideoPlayer.isLooping = true;
        CTIVideoImage.texture = videoRenderTexture;
    }

    void InitRenderTexture()
    {
        if (videoRenderTexture != null)
        {
            ReleaseRenderTexture();
        }

        videoRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, 0);
        videoRenderTexture.Create();
    }

    void ReleaseRenderTexture()
    {
        CTIVideoImage.texture = null;

        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            videoRenderTexture = null;
        }
    }

    void PrepareCTIAsset()
    {
        loadedType = CTIType.None;

        if (Directory.Exists(CTI_PATH))
        {
            if(!File.Exists(currentCTIfileName))
            {
                // if the file does not exist, see if there is a usable one and use that
                var fileNames = GetCTIFileNames();
                if(fileNames != null &&  fileNames.Length > 0)
                {
                    currentCTIfileName = Path.Combine(CTI_PATH, fileNames[0]);
                }
            }

            if (File.Exists(currentCTIfileName))
            {
                CTIType ctiType = CTIType.None;

                foreach (var extension in CTI_VIDEO_EXTENSIONS)
                {
                    if (currentCTIfileName.Contains(extension))
                    {
                        ctiType = CTIType.Video;
                        break;
                    }
                }

                if (ctiType == CTIType.None)
                {
                    foreach (var extension in CTI_IMAGE_EXTENSIONS)
                    {
                        if (currentCTIfileName.Contains(extension))
                        {
                            ctiType = CTIType.Image;
                            break;
                        }
                    }
                }

                if (ctiType == CTIType.Image)
                {
                    // image time
                    CTIImage.enabled = false;

                    byte[] pngBytes = File.ReadAllBytes(currentCTIfileName);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.LoadImage(pngBytes);
                    tex.Apply(false, true);
                    CTIImage.texture = tex;
                    loadedType = CTIType.Image;
                }
                else if (ctiType == CTIType.Video)
                {
                    // video time
                    CTIVideoImage.enabled = false;

                    VideoPlayer.source = VideoSource.Url;
                    VideoPlayer.url = currentCTIfileName;
                    VideoPlayer.Prepare();
                    loadedType = CTIType.Video;
                }
            }
        }

        if (loadedType == CTIType.None)
        {
            Debug.Log($"Could not find any CTI assets to load at {CTI_PATH}");
        }
    }

    public static string[] GetCTIFileNames()
    {
        if (Directory.Exists(CTI_PATH))
        {
            string[] files = Directory.GetFiles(CTI_PATH);
            List<string> filesWithExtension = new List<string>();

            foreach (var file in files)
            {
                foreach (var ext in CTI_EXTENSIONS)
                {
                    if (file.Contains(ext) && !file.Contains(".meta"))
                    {
                        filesWithExtension.Add(file.Replace(CTI_PATH, "").Replace("\\", ""));
                        break;
                    }
                }
            }

            return filesWithExtension.ToArray();
        }

        return null;
    }
}