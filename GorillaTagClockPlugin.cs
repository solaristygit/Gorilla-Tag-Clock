using System;
using System.Collections;
using BepInEx;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaTagClock
{
    [BepInPlugin(
        "com.gorillatagclock.localclock",
        "GorillaTagClock",
        "1.0.0"
    )]
    public class GorillaTagClockPlugin : BaseUnityPlugin
    {
        private const string BundleName =
            "gorillatagclock";

        private const string ClockAssetName =
            "GorillaTagClock";

        private GameObject clock;

        private Text timeText;

        private float updateTimer;

        private bool loaded;

        // =====================================================
        // CLOCK POSITION
        // =====================================================

        /*
         * These are LOCAL coordinates relative to the
         * cosmetics anchor.
         *
         * X = left/right
         * Y = up/down
         * Z = forward/back
         */

        private readonly Vector3 clockPosition =
            new Vector3(
                0.65f,
                0.35f,
                -0.25f
            );

        private readonly Vector3 clockRotation =
            new Vector3(
                0f,
                0f,
                0f
            );

        private readonly Vector3 clockScale =
            new Vector3(
                0.25f,
                0.25f,
                0.25f
            );

        // =====================================================
        // STARTUP
        // =====================================================

        private void Awake()
        {
            Logger.LogInfo(
                "[GorillaTagClock] Loading..."
            );

            StartCoroutine(
                LoadClock()
            );
        }

        // =====================================================
        // LOAD ASSETBUNDLE
        // =====================================================

        private IEnumerator LoadClock()
        {
            string bundlePath =
                System.IO.Path.Combine(
                    Paths.PluginPath,
                    "GorillaTagClock",
                    BundleName
                );

            Logger.LogInfo(
                "[GorillaTagClock] Bundle path: " +
                bundlePath
            );

            if (!System.IO.File.Exists(bundlePath))
            {
                Logger.LogError(
                    "[GorillaTagClock] AssetBundle not found!"
                );

                Logger.LogError(
                    "[GorillaTagClock] Expected: " +
                    bundlePath
                );

                yield break;
            }

            AssetBundleCreateRequest request =
                AssetBundle.LoadFromFileAsync(
                    bundlePath
                );

            yield return request;

            AssetBundle bundle =
                request.assetBundle;

            if (bundle == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Failed to load AssetBundle."
                );

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] AssetBundle loaded."
            );

            AssetBundleRequest assetRequest =
                bundle.LoadAssetAsync<GameObject>(
                    ClockAssetName
                );

            yield return assetRequest;

            GameObject prefab =
                assetRequest.asset as GameObject;

            if (prefab == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Clock prefab not found."
                );

                bundle.Unload(false);

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] Clock model loaded."
            );

            // Keep the bundle loaded because the instantiated
            // object uses assets from it.
            CreateClock(prefab);
        }

        // =====================================================
        // FIND COSMETICS
        // =====================================================

        private Transform FindCosmeticsAnchor()
        {
            if (CosmeticsController.instance != null)
            {
                Transform controller =
                    CosmeticsController.instance.transform;

                return controller;
            }

            return null;
        }

        // =====================================================
        // CREATE CLOCK
        // =====================================================

        private void CreateClock(
            GameObject prefab
        )
        {
            if (clock != null)
                return;

            Transform anchor =
                FindCosmeticsAnchor();

            if (anchor == null)
            {
                Logger.LogInfo(
                    "[GorillaTagClock] Waiting for cosmetics..."
                );

                StartCoroutine(
                    WaitForCosmetics(prefab)
                );

                return;
            }

            clock =
                Instantiate(prefab);

            clock.name =
                "GorillaTagClock_Local";

            clock.transform.SetParent(
                anchor,
                false
            );

            clock.transform.localPosition =
                clockPosition;

            clock.transform.localRotation =
                Quaternion.Euler(
                    clockRotation
                );

            clock.transform.localScale =
                clockScale;

            AddLocalClockDisplay();

            loaded = true;

            Logger.LogInfo(
                "[GorillaTagClock] Clock placed beside cosmetics."
            );
        }

        private IEnumerator WaitForCosmetics(
            GameObject prefab
        )
        {
            while (
                CosmeticsController.instance == null
            )
            {
                yield return null;
            }

            CreateClock(prefab);
        }

        // =====================================================
        // LOCAL DIGITAL DISPLAY
        // =====================================================

        private void AddLocalClockDisplay()
        {
            GameObject display =
                new GameObject(
                    "LocalTimeDisplay"
                );

            display.transform.SetParent(
                clock.transform,
                false
            );

            /*
             * You may need to adjust this depending on the
             * exact orientation of the FBX.
             *
             * The display is deliberately a child of the
             * physical clock model.
             */

            display.transform.localPosition =
                new Vector3(
                    0f,
                    0.15f,
                    -0.35f
                );

            display.transform.localRotation =
                Quaternion.identity;

            display.transform.localScale =
                Vector3.one * 0.002f;

            Canvas canvas =
                display.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.WorldSpace;

            CanvasScaler scaler =
                display.AddComponent<CanvasScaler>();

            scaler.dynamicPixelsPerUnit =
                100f;

            timeText =
                display.AddComponent<Text>();

            timeText.font =
                Resources.GetBuiltinResource<Font>(
                    "Arial.ttf"
                );

            timeText.fontSize =
                70;

            timeText.fontStyle =
                FontStyle.Bold;

            timeText.alignment =
                TextAnchor.MiddleCenter;

            timeText.color =
                new Color(
                    0.7f,
                    1f,
                    1f
                );

            timeText.raycastTarget =
                false;

            RectTransform rect =
                display.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(
                    500f,
                    150f
                );

            UpdateTime();
        }

        // =====================================================
        // LOCAL TIME
        // =====================================================

        private void Update()
        {
            if (!loaded)
                return;

            if (timeText == null)
                return;

            updateTimer +=
                Time.deltaTime;

            if (updateTimer >= 1f)
            {
                updateTimer = 0f;

                UpdateTime();
            }
        }

        private void UpdateTime()
        {
            if (timeText == null)
                return;

            DateTime localTime =
                DateTime.Now;

            timeText.text =
                localTime.ToString(
                    "h:mm:ss tt"
                );
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        private void OnDestroy()
        {
            if (clock != null)
            {
                Destroy(clock);

                clock = null;
            }

            timeText = null;
        }
    }
}
