using System;
using System.Collections;
using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using GorillaNetworking;

namespace GorillaTagClock
{
    [BepInPlugin(
        "com.gorillatagclock.localclock",
        "GorillaTagClock",
        "1.1.0"
    )]
    public class GorillaTagClockPlugin : BaseUnityPlugin
    {
        private AssetBundle bundle;
        private GameObject clock;
        private Text timeText;

        private float timer;

        private void Awake()
        {
            Logger.LogInfo(
                "[GorillaTagClock] Plugin loaded!"
            );

            StartCoroutine(LoadClock());
        }

        private IEnumerator LoadClock()
        {
            string path = Path.Combine(
                Paths.PluginPath,
                "GorillaTagClock",
                "gorillatagclock"
            );

            Logger.LogInfo(
                "[GorillaTagClock] Looking for bundle:"
            );

            Logger.LogInfo(path);

            if (!File.Exists(path))
            {
                Logger.LogError(
                    "[GorillaTagClock] BUNDLE DOES NOT EXIST!"
                );

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] Bundle found."
            );

            AssetBundleCreateRequest request =
                AssetBundle.LoadFromFileAsync(path);

            yield return request;

            bundle = request.assetBundle;

            if (bundle == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] AssetBundle failed to load!"
                );

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] AssetBundle loaded."
            );

            string[] assets =
                bundle.GetAllAssetNames();

            Logger.LogInfo(
                "[GorillaTagClock] Assets in bundle: "
                + assets.Length
            );

            foreach (string asset in assets)
            {
                Logger.LogInfo(
                    "[GorillaTagClock] Asset: "
                    + asset
                );
            }

            GameObject prefab = null;

            foreach (string assetName in assets)
            {
                AssetBundleRequest assetRequest =
                    bundle.LoadAssetAsync<GameObject>(
                        assetName
                    );

                yield return assetRequest;

                GameObject found =
                    assetRequest.asset as GameObject;

                if (found != null)
                {
                    prefab = found;

                    Logger.LogInfo(
                        "[GorillaTagClock] Found clock asset: "
                        + assetName
                    );

                    break;
                }
            }

            if (prefab == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] NO GAMEOBJECT FOUND IN BUNDLE!"
                );

                yield break;
            }

            StartCoroutine(
                WaitForCosmetics(prefab)
            );
        }

        private IEnumerator WaitForCosmetics(
            GameObject prefab
        )
        {
            Logger.LogInfo(
                "[GorillaTagClock] Waiting for CosmeticsController..."
            );

            while (
                CosmeticsController.instance == null
            )
            {
                yield return new WaitForSeconds(1f);
            }

            Logger.LogInfo(
                "[GorillaTagClock] CosmeticsController found!"
            );

            Transform anchor =
                CosmeticsController.instance.transform;

            if (anchor == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Cosmetics transform is null!"
                );

                yield break;
            }

            CreateClock(
                prefab,
                anchor
            );
        }

        private void CreateClock(
            GameObject prefab,
            Transform anchor
        )
        {
            if (clock != null)
                return;

            clock = Instantiate(
                prefab
            );

            clock.name =
                "GorillaTagClock_Local";

            clock.transform.SetParent(
                anchor,
                false
            );

            /*
             * START HERE.
             *
             * We can adjust these once we know the
             * clock is successfully appearing.
             */

            clock.transform.localPosition =
                new Vector3(
                    0.8f,
                    0.5f,
                    0f
                );

            clock.transform.localRotation =
                Quaternion.identity;

            clock.transform.localScale =
                Vector3.one * 0.25f;

            AddTimeDisplay();

            Logger.LogInfo(
                "[GorillaTagClock] CLOCK CREATED!"
            );
        }

        private void AddTimeDisplay()
        {
            GameObject display =
                new GameObject(
                    "ClockTime"
                );

            display.transform.SetParent(
                clock.transform,
                false
            );

            display.transform.localPosition =
                new Vector3(
                    0f,
                    0.1f,
                    -0.3f
                );

            Canvas canvas =
                display.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.WorldSpace;

            display.AddComponent<CanvasScaler>();

            timeText =
                display.AddComponent<Text>();

            timeText.font =
                Resources.GetBuiltinResource<Font>(
                    "Arial.ttf"
                );

            timeText.fontSize = 80;

            timeText.alignment =
                TextAnchor.MiddleCenter;

            timeText.fontStyle =
                FontStyle.Bold;

            timeText.color =
                Color.white;

            timeText.raycastTarget =
                false;

            RectTransform rect =
                display.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(
                    500f,
                    150f
                );

            display.transform.localScale =
                Vector3.one * 0.002f;

            UpdateTime();
        }

        private void Update()
        {
            if (timeText == null)
                return;

            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                timer = 0f;
                UpdateTime();
            }
        }

        private void UpdateTime()
        {
            if (timeText == null)
                return;

            timeText.text =
                DateTime.Now.ToString(
                    "h:mm:ss tt"
                );
        }

        private void OnDestroy()
        {
            if (clock != null)
                Destroy(clock);

            if (bundle != null)
                bundle.Unload(false);
        }
    }
}
