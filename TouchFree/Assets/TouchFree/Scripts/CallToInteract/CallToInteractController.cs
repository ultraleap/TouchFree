﻿using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
using Ultraleap.ScreenControl.Client.Connection;
using Ultraleap.ScreenControl.Client;
using System.Collections;

public class CallToInteractController : MonoBehaviour
{
    public enum CTIType
    {
        NONE,
        IMAGE,
        VIDEO
    }

    public static event Action OnCTIActive;
    public static event Action OnCTIInactive;

    public RawImage CTIImage;
    public RawImage CTIVideoImage;
    public VideoPlayer VideoPlayer;

    CTIType loadedType;
    RenderTexture videoRenderTexture;

    bool isShowing;

    private static readonly string CTI_PATH = Path.Combine(Application.streamingAssetsPath, "CallToInteract");

    private readonly string[] VIDEO_EXTENSIONS = new string[] { ".webm", ".mp4" };
    private readonly string[] IMAGE_EXTENSIONS = new string[] { ".png" };

    public void UpdateCTISettings()
    {
        // TODO: this should bhe replaced by an event listener to the config system
        SetupCTI();
    }

    void OnEnable()
    {
        ConnectionManager.HandFound += OnHandEnter;
        ConnectionManager.HandsLost += OnAllHandsExit;
        ConnectionManager.TransmitInputAction += HandleInputAction;
        ScreenManager.Instance.UIActivated += UIAcitvated;

        isShowing = false;
        SetupCTI(true);
    }

    void OnDisable()
    {
        ConnectionManager.HandFound -= OnHandEnter;
        ConnectionManager.HandsLost -= OnAllHandsExit;
        ConnectionManager.TransmitInputAction -= HandleInputAction;
        ScreenManager.Instance.UIActivated -= UIAcitvated;

        if (videoRenderTexture != null)
        {
            ReleaseRenderTexture();
        }
        if (CTIImage.texture != null)
        {
            Destroy(CTIImage.texture);
            CTIImage.texture = null;
        }

        StopAllCoroutines();
        delayedSetupCoroutine = null;
        showAfterHandsLostCoroutine = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_immediate">Force the Setup. Otherwise it is provided on a cooldown to
    /// prevent the loading of video assets on every update of values</param>
    void SetupCTI(bool _immediate = false)
    {
        // If the CTI isn't enabled in the config, disable ourselves and exit out.
        if (!CallToInteractConfig.Config.Enabled)
        {
            loadedType = CTIType.NONE;
            return;
        }

        if(!_immediate)
        {
            if (delayedSetupCoroutine == null)
            {
                delayedSetupCoroutine = StartCoroutine(DelayedSetup());
            }
            return;
        }

        HideCTI();
        PrepareCTIAsset();

        if (loadedType == CTIType.VIDEO)
        {
            InitVideoPlayer();
        }
    }

    Coroutine delayedSetupCoroutine = null;
    IEnumerator DelayedSetup()
    {
        yield return new WaitForSeconds(0.5f);
        SetupCTI(true);
        delayedSetupCoroutine = null;
    }

    void UIAcitvated()
    {
        if(isShowing)
        {
            HideCTI();
        }
    }

    void OnHandEnter()
    {
        if (isShowing && CallToInteractConfig.Config.hideType == HideRequirement.PRESENT)
        {
            HideCTI();
        }
    }

    void OnAllHandsExit()
    {
        if (!isShowing && showAfterHandsLostCoroutine != null)
        {
            showAfterHandsLostCoroutine = StartCoroutine(ShowAfterHandsLost());
        }
    }

    void HandleInputAction(ClientInputAction _inputAction)
    {
        if (_inputAction.InputType == InputType.UP)
        {
            if (isShowing && CallToInteractConfig.Config.hideType == HideRequirement.INTERACTION)
            {
                HideCTI();
            }
        }
    }

    Coroutine showAfterHandsLostCoroutine;
    IEnumerator ShowAfterHandsLost()
    {
        yield return new WaitForSeconds(CallToInteractConfig.Config.ShowTimeAfterNoHandPresent);
        ShowCTI();
        showAfterHandsLostCoroutine = null;
    }

    void ShowCTI()
    {
        if(ScreenManager.Instance.isActive || loadedType == CTIType.NONE)
        {
            return;
        }

        isShowing = true;

        if (loadedType == CTIType.IMAGE)
        {
            CTIImage.enabled = true;
        }
        else if (loadedType == CTIType.VIDEO)
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
        isShowing = false;

        if (loadedType == CTIType.IMAGE)
        {
            CTIImage.enabled = false;
        }
        else if (loadedType == CTIType.VIDEO)
        {
            CTIVideoImage.enabled = false;
            VideoPlayer.Stop();
        }

        OnCTIInactive?.Invoke();
    }

    #region Asset Preparation

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
        // Finally show the video image now that the stutter frames are gone!
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
        loadedType = CTIType.NONE;

        if (File.Exists(CallToInteractConfig.Config.CurrentFileName))
        {
            CTIType ctiType = CTIType.NONE;

            foreach (var extension in VIDEO_EXTENSIONS)
            {
                if (CallToInteractConfig.Config.CurrentFileName.Contains(extension))
                {
                    ctiType = CTIType.VIDEO;
                    break;
                }
            }

            if (ctiType == CTIType.NONE)
            {
                foreach (var extension in IMAGE_EXTENSIONS)
                {
                    if (CallToInteractConfig.Config.CurrentFileName.Contains(extension))
                    {
                        ctiType = CTIType.IMAGE;
                        break;
                    }
                }
            }

            if (ctiType == CTIType.IMAGE)
            {
                // image time
                CTIImage.enabled = false;

                byte[] pngBytes = File.ReadAllBytes(CallToInteractConfig.Config.CurrentFileName);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(pngBytes);
                tex.Apply(false, true);
                CTIImage.texture = tex;
                loadedType = CTIType.IMAGE;
            }
            else if (ctiType == CTIType.VIDEO)
            {
                // video time
                CTIVideoImage.enabled = false;

                VideoPlayer.source = VideoSource.Url;
                VideoPlayer.url = CallToInteractConfig.Config.CurrentFileName;
                VideoPlayer.Prepare();
                loadedType = CTIType.VIDEO;
            }
        }

        if (loadedType == CTIType.NONE)
        {
            Debug.Log($"Could not find any CTI assets to load at {CTI_PATH}");
        }
    }
    #endregion
}