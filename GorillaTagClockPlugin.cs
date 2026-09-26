using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace GorillaTagClock
{
    [BepInPlugin(
        "com.gorillatagclock.localclock",
        "GorillaTagClock",
        "1.0.0"
    )]
    public class GorillaTagClockPlugin : BaseUnityPlugin
    {
        private AssetBundle clockBundle;
        private GameObject clockObject;

        private ConfigEntry<float> positionX;
        private ConfigEntry<float> positionY;
        private ConfigEntry<float> positionZ;

        private ConfigEntry<float> rotationX;
        private ConfigEntry<float> rotationY;
        private ConfigEntry<float> rotationZ;

        private ConfigEntry<float> scale;

        private void Awake()
        {
            positionX = Config.Bind(
                "Clock",
                "PositionX",
                0.5f,
                "Clock X position."
            );

            positionY = Config.Bind(
                "Clock",
                "PositionY",
                0.5f,
                "Clock Y position."
            );

            positionZ = Config.Bind(
                "Clock",
                "PositionZ",
                0f,
                "Clock Z position."
            );

            rotationX = Config.Bind(
                "Clock",
                "RotationX",
                0f,
                "Clock X rotation."
            );

            rotationY = Config.Bind(
                "Clock",
                "RotationY",
                0f,
                "Clock Y rotation."
            );

            rotationZ = Config.Bind(
                "Clock",
                "RotationZ",
                0f,
                "Clock Z rotation."
            );

            scale = Config.Bind(
                "Clock",
                "Scale",
                0.25f,
                "Clock scale."
            );

            Logger.LogInfo(
                "[GorillaTagClock] Loading..."
            );

            StartCoroutine(LoadClock());
        }

        private IEnumerator LoadClock()
        {
            string bundlePath = Path.Combine(
                Paths.PluginPath,
                "GorillaTagClock",
                "gorillatagclock"
            );

            Logger.LogInfo(
                "[GorillaTagClock] Bundle path: " +
                bundlePath
            );

            if (!File.Exists(bundlePath))
            {
                Logger.LogError(
                    "[GorillaTagClock] AssetBundle not found!"
                );

                yield break;
            }

            AssetBundleCreateRequest request =
                AssetBundle.LoadFromFileAsync(
                    bundlePath
                );

            yield return request;

            clockBundle = request.assetBundle;

            if (clockBundle == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Failed to load AssetBundle."
                );

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] AssetBundle loaded."
            );

            string[] assets =
                clockBundle.GetAllAssetNames();

            foreach (string asset in assets)
            {
                Logger.LogInfo(
                    "[GorillaTagClock] Asset: " +
                    asset
                );
            }

            AssetBundleRequest prefabRequest =
                clockBundle.LoadAssetAsync<GameObject>(
                    "assets/clock/gorillatagclock.prefab"
                );

            yield return prefabRequest;

            GameObject prefab =
                prefabRequest.asset as GameObject;

            if (prefab == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Could not load clock prefab."
                );

                yield break;
            }

            Logger.LogInfo(
                "[GorillaTagClock] Clock prefab loaded!"
            );

            StartCoroutine(
                SpawnClockWhenReady(prefab)
            );
        }

        private IEnumerator SpawnClockWhenReady(
            GameObject prefab
        )
        {
            /*
             * Wait until Gorilla Tag has finished loading
             * the world.
             */
            yield return new WaitForSeconds(5f);

            SpawnClock(prefab);
        }

        private void SpawnClock(
            GameObject prefab
        )
        {
            if (clockObject != null)
                return;

            clockObject =
                Instantiate(prefab);

            clockObject.name =
                "GorillaTagClock_Local";

            /*
             * Initial world-space position.
             *
             * These values are intentionally configurable
             * through the BepInEx config file.
             */
            clockObject.transform.position =
                new Vector3(
                    positionX.Value,
                    positionY.Value,
                    positionZ.Value
                );

            clockObject.transform.rotation =
                Quaternion.Euler(
                    rotationX.Value,
                    rotationY.Value,
                    rotationZ.Value
                );

            clockObject.transform.localScale =
                Vector3.one * scale.Value;

            Logger.LogInfo(
                "[GorillaTagClock] CLOCK SPAWNED!"
            );
        }

        private void OnDestroy()
        {
            if (clockObject != null)
            {
                Destroy(clockObject);
                clockObject = null;
            }

            if (clockBundle != null)
            {
                clockBundle.Unload(false);
                clockBundle = null;
            }
        }
    }
}
