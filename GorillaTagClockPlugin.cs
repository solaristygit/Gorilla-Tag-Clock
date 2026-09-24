using System;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaTagClock
{
    [BepInPlugin(
        "com.gorillatagclock.localclock",
        "GorillaTagClock",
        "3.0.0"
    )]
    public class GorillaTagClockPlugin : BaseUnityPlugin
    {
        private GameObject clockObject;
        private Text clockText;

        private float timer;

        private void Awake()
        {
            Logger.LogInfo(
                "[GorillaTagClock] PLUGIN LOADED"
            );
        }

        private void Start()
        {
            Logger.LogInfo(
                "[GorillaTagClock] Starting clock..."
            );

            Invoke(
                nameof(CreateClock),
                5f
            );
        }

        private void CreateClock()
        {
            if (clockObject != null)
                return;

            Camera cam = Camera.main;

            if (cam == null)
            {
                Logger.LogError(
                    "[GorillaTagClock] Main camera not found."
                );

                return;
            }

            Logger.LogInfo(
                "[GorillaTagClock] Camera found."
            );

            // Create the clock.
            clockObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            clockObject.name =
                "GorillaTagClock_Local";

            // Put it directly in front of the
            // player's camera.
            clockObject.transform.SetParent(
                cam.transform,
                false
            );

            clockObject.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    1.5f
                );

            clockObject.transform.localRotation =
                Quaternion.identity;

            clockObject.transform.localScale =
                new Vector3(
                    0.7f,
                    0.35f,
                    0.08f
                );

            Renderer renderer =
                clockObject.GetComponent<Renderer>();

            renderer.material =
                CreateMaterial(
                    new Color(
                        0.01f,
                        0.01f,
                        0.01f
                    )
                );

            Collider collider =
                clockObject.GetComponent<Collider>();

            if (collider != null)
                Destroy(collider);

            CreateDisplay();

            CreateClockText();

            UpdateClock();

            Logger.LogInfo(
                "[GorillaTagClock] CLOCK CREATED SUCCESSFULLY"
            );
        }

        private void CreateDisplay()
        {
            GameObject display =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

            display.name =
                "ClockDisplay";

            display.transform.SetParent(
                clockObject.transform,
                false
            );

            display.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -0.041f
                );

            display.transform.localScale =
                new Vector3(
                    0.9f,
                    0.75f,
                    0.02f
                );

            Renderer renderer =
                display.GetComponent<Renderer>();

            renderer.material =
                CreateMaterial(
                    new Color(
                        0.005f,
                        0.02f,
                        0.025f
                    )
                );

            Collider collider =
                display.GetComponent<Collider>();

            if (collider != null)
                Destroy(collider);
        }

        private void CreateClockText()
        {
            GameObject textObject =
                new GameObject(
                    "ClockTime"
                );

            textObject.transform.SetParent(
                clockObject.transform,
                false
            );

            textObject.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -0.06f
                );

            Canvas canvas =
                textObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.WorldSpace;

            CanvasScaler scaler =
                textObject.AddComponent<CanvasScaler>();

            scaler.dynamicPixelsPerUnit =
                100f;

            clockText =
                textObject.AddComponent<Text>();

            clockText.font =
                Resources.GetBuiltinResource<Font>(
                    "Arial.ttf"
                );

            clockText.fontSize = 80;

            clockText.fontStyle =
                FontStyle.Bold;

            clockText.alignment =
                TextAnchor.MiddleCenter;

            clockText.color =
                new Color(
                    0.4f,
                    1f,
                    1f
                );

            clockText.raycastTarget =
                false;

            clockText.horizontalOverflow =
                HorizontalWrapMode.Overflow;

            clockText.verticalOverflow =
                VerticalWrapMode.Overflow;

            RectTransform rect =
                textObject.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(
                    500f,
                    180f
                );
        }

        private Material CreateMaterial(
            Color color
        )
        {
            Shader shader =
                Shader.Find("Standard");

            if (shader == null)
            {
                shader =
                    Shader.Find("Unlit/Color");
            }

            Material material =
                new Material(shader);

            material.color =
                color;

            return material;
        }

        private void Update()
        {
            if (clockText == null)
                return;

            timer += Time.deltaTime;

            if (timer >= 1f)
            {
                timer = 0f;

                UpdateClock();
            }
        }

        private void UpdateClock()
        {
            if (clockText == null)
                return;

            // This is the LOCAL time of the
            // computer running Gorilla Tag.
            DateTime localTime =
                DateTime.Now;

            clockText.text =
                localTime.ToString(
                    "h:mm:ss tt"
                );
        }

        private void OnDestroy()
        {
            if (clockObject != null)
            {
                Destroy(clockObject);
                clockObject = null;
            }

            clockText = null;
        }
    }
}
