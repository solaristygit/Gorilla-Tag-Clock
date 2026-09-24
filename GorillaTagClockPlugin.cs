```csharp
using System;
using BepInEx;
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
        private GameObject clockObject;
        private Text clockText;

        private float updateTimer;

        // Position of the clock next to the wardrobe.
        private readonly Vector3 clockPosition =
            new Vector3(0.35f, 0.25f, 0f);

        private void Awake()
        {
            Logger.LogInfo(
                "GorillaTagClock loaded!"
            );
        }

        private void Start()
        {
            // Wait for the cosmetics area to load,
            // then keep checking until we find it.
            InvokeRepeating(
                nameof(TryCreateClock),
                3f,
                5f
            );
        }

        private void TryCreateClock()
        {
            // Don't create multiple clocks.
            if (clockObject != null)
                return;

            GameObject wardrobe = FindWardrobe();

            if (wardrobe == null)
            {
                Logger.LogInfo(
                    "GorillaTagClock: Waiting for wardrobe..."
                );

                return;
            }

            CreateClock(wardrobe.transform);
        }

        private GameObject FindWardrobe()
        {
            string[] possibleNames =
            {
                "Wardrobe",
                "CosmeticWardrobe",
                "CosmeticsWardrobe",
                "Cosmetics",
                "CosmeticStand",
                "CosmeticStandManager"
            };

            foreach (string name in possibleNames)
            {
                GameObject found = GameObject.Find(name);

                if (found != null)
                    return found;
            }

            // Fallback search for an object containing
            // "wardrobe" in its name.
            GameObject[] allObjects =
                FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj == null)
                    continue;

                string objectName =
                    obj.name.ToLower();

                if (objectName.Contains("wardrobe"))
                    return obj;
            }

            return null;
        }

        private void CreateClock(Transform wardrobe)
        {
            clockObject =
                new GameObject("GorillaTagClock_Local");

            // Keep the clock attached to the wardrobe.
            clockObject.transform.SetParent(
                wardrobe,
                false
            );

            clockObject.transform.localPosition =
                clockPosition;

            clockObject.transform.localRotation =
                Quaternion.identity;

            clockObject.transform.localScale =
                Vector3.one * 0.0025f;

            // -------------------------
            // World-space Canvas
            // -------------------------

            Canvas canvas =
                clockObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.WorldSpace;

            CanvasScaler scaler =
                clockObject.AddComponent<CanvasScaler>();

            scaler.dynamicPixelsPerUnit = 10f;

            clockObject.AddComponent<GraphicRaycaster>();

            // -------------------------
            // Background
            // -------------------------

            GameObject backgroundObject =
                new GameObject("ClockBackground");

            backgroundObject.transform.SetParent(
                clockObject.transform,
                false
            );

            Image background =
                backgroundObject.AddComponent<Image>();

            background.color =
                new Color(0f, 0f, 0f, 0.75f);

            RectTransform backgroundRect =
                backgroundObject.GetComponent<RectTransform>();

            backgroundRect.sizeDelta =
                new Vector2(420f, 110f);

            backgroundRect.localPosition =
                Vector3.zero;

            // -------------------------
            // Clock Text
            // -------------------------

            GameObject textObject =
                new GameObject("ClockText");

            textObject.transform.SetParent(
                clockObject.transform,
                false
            );

            clockText =
                textObject.AddComponent<Text>();

            clockText.font =
                Resources.GetBuiltinResource<Font>(
                    "Arial.ttf"
                );

            clockText.fontSize = 55;

            clockText.alignment =
                TextAnchor.MiddleCenter;

            clockText.color =
                Color.white;

            clockText.horizontalOverflow =
                HorizontalWrapMode.Overflow;

            clockText.verticalOverflow =
                VerticalWrapMode.Overflow;

            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            textRect.sizeDelta =
                new Vector2(420f, 110f);

            textRect.localPosition =
                Vector3.zero;

            // Immediately show the user's local time.
            UpdateClock();

            Logger.LogInfo(
                "GorillaTagClock: Local clock created."
            );
        }

        private void Update()
        {
            if (clockText == null)
                return;

            updateTimer += Time.deltaTime;

            if (updateTimer >= 1f)
            {
                updateTimer = 0f;

                // Updates using the local computer time.
                UpdateClock();
            }
        }

        private void UpdateClock()
        {
            if (clockText == null)
                return;

            // DateTime.Now gets the LOCAL time of
            // the computer running Gorilla Tag.
            DateTime localTime = DateTime.Now;

            // Example:
            // 8:55:32 PM
            clockText.text =
                localTime.ToString("h:mm:ss tt");
        }

        private void OnDestroy()
        {
            if (clockObject != null)
            {
                Destroy(clockObject);

                clockObject = null;
                clockText = null;
            }
        }
    }
}
```
