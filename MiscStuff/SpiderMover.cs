using UnityEngine;
using UnityEngine.UI;

namespace MysteryDice.MiscStuff
{
    
    //GOD I had SOOOO much help with this
    [RequireComponent(typeof(RectTransform))]
    public class SpiderPathUI : MonoBehaviour
    {
        [Header("Canvas")]
        public Canvas rootCanvas;
        
        [Header("Versions")]
        public GameObject cuteOverlay;
        public GameObject SpiderOverlay;

        [Header("Timing")]
        public Vector2 roamDuration = new Vector2(1.5f, 3.0f);
        public float exitDuration = 1.25f;
        public Vector2 dwellOffscreen = new Vector2(0.4f, 120.0f);

        [Header("Curviness")]
        [Range(0f, 1f)] public float controlScatter = 0.55f;
        public int bezierSamplesPerSecond = 60;

        [Header("Bounds")]
        public bool autoMarginFromSize = true;
        public float margin = 50f;
        
        RectTransform rt;
        RectTransform canvasRT;

        Vector2 boundsMin, boundsMax;

        Vector2 p0, p1, p2, p3;
        float t, segDuration;
        bool exiting;

        float offscreenTimer;

        private bool lastState;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
            canvasRT = rootCanvas ? rootCanvas.GetComponent<RectTransform>() : null;

            if (!canvasRT)
            {
                enabled = false; return;
            }
            
            lastState = CurrentSpiderSafe();
            if (cuteOverlay) cuteOverlay.SetActive(lastState);
            SpiderOverlay.SetActive(!lastState);
            
            RebuildBounds();
            BeginRoam();
        }

        void OnRectTransformDimensionsChange()
        {
            if (canvasRT) RebuildBounds();
        }

        void RebuildBounds()
        {
            Vector2 half = canvasRT.rect.size * 0.5f;
            float m = autoMarginFromSize ? AutoMargin(rt) : margin;
            boundsMin = -half - new Vector2(m, m);
            boundsMax =  half + new Vector2(m, m);
        }

        float AutoMargin(RectTransform r)
        {
            Vector2 s = r.rect.size;
            return 0.5f * Mathf.Max(s.x, s.y);
        }
        
        static bool CurrentSpiderSafe()
        {
            var inst = IngamePlayerSettings.Instance;
            if (inst == null) return false;

            bool? live  = inst.unsavedSettings?.spiderSafeMode;
            bool? saved = inst.settings?.spiderSafeMode;

            return live ?? saved ?? false;
        }
        void Update()
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase ||
             StartOfRound.Instance.shipIsLeaving)
            {
                Destroy(this.gameObject);
                return;
            }
            bool current = CurrentSpiderSafe();
            if (current != lastState)
            {
                lastState = current;
                cuteOverlay.SetActive(current);
                SpiderOverlay.SetActive(!current);
            }
            float dt = Time.unscaledDeltaTime;
            if (offscreenTimer > 0f)
            {
                offscreenTimer -= dt;
                if (offscreenTimer <= 0f) BeginRoam();
                return;
            }

            if (segDuration <= 0f) return;

            t += dt / segDuration;
            if (t >= 1f)
            {
                SetOnCurve(1f);
                if (!exiting) BeginExit();
                else BeginOffscreenDwell();
                return;
            }

            SetOnCurve(t);
        }
        void BeginRoam()
        {
            exiting = false;
            segDuration = Random.Range(roamDuration.x, roamDuration.y);
            t = 0f;

            Vector2 start = GetCurrentLocal();
            p0 = start;
            p3 = RandomPointIn(boundsMin, boundsMax);
            MakeControls(p0, p3, out p1, out p2, onExit:false);
        }

        void BeginExit()
        {
            exiting = true;
            segDuration = exitDuration;
            t = 0f;

            Vector2 start = GetCurrentLocal();
            p0 = start;
            p3 = RandomOffscreenPoint(boundsMin, boundsMax);

            MakeControls(p0, p3, out p1, out p2, onExit:true);
        }

        void BeginOffscreenDwell()
        {
            offscreenTimer = Random.Range(dwellOffscreen.x,dwellOffscreen.y);
        }
        void SetOnCurve(float tt)
        {
            Vector2 pos = Bezier(p0, p1, p2, p3, tt);
            Vector2 tan = BezierTangent(p0, p1, p2, p3, tt);

            rt.anchoredPosition = pos;
            if (tan.sqrMagnitude > 1e-6f)
                rt.up = tan.normalized;
        }

        Vector2 GetCurrentLocal()
        {
            return rt.anchoredPosition;
            
        }

        
        //fuck if I know this shit, this was someone else so if it breaks... I cry
        static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float u = 1f - t;
            return  u*u*u * a
                  + 3f*u*u*t * b
                  + 3f*u*t*t * c
                  +     t*t*t * d;
        }
        
        static Vector2 BezierTangent(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float u = 1f - t;
            return 3f*u*u*(b - a) + 6f*u*t*(c - b) + 3f*t*t*(d - c);
        }

        void MakeControls(Vector2 start, Vector2 end, out Vector2 c1, out Vector2 c2, bool onExit)
        {
            Vector2 mid = (start + end) * 0.5f;
            Vector2 dir = (end - start);
            float len = dir.magnitude + 1e-3f;
            Vector2 n = new Vector2(-dir.y, dir.x) / len;
            float scatter = controlScatter * Mathf.Min(len, canvasRT.rect.size.magnitude);
            float s1 = Random.Range(-scatter, scatter);
            float s2 = Random.Range(-scatter, scatter);
            c1 = Vector2.Lerp(start, mid, 0.33f) + n * s1;
            c2 = Vector2.Lerp(mid, end, 0.66f) + n * s2;
            if (onExit)
            {
                c1 = Vector2.Lerp(c1, end, 0.15f);
                c2 = Vector2.Lerp(c2, end, 0.25f);
            }
            if (!onExit)
            {
                c1 = ClampTo(boundsMin, boundsMax, c1);
                c2 = ClampTo(boundsMin, boundsMax, c2);
            }
        }

        Vector2 RandomPointIn(Vector2 min, Vector2 max)
        {
            return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        }

        Vector2 RandomOffscreenPoint(Vector2 min, Vector2 max)
        {
            float left = min.x, right = max.x, bottom = min.y, top = max.y;

            int side = Random.Range(0, 4);
            switch (side)
            {
                case 0: return new Vector2(left, Random.Range(bottom, top));
                case 1: return new Vector2(right, Random.Range(bottom, top));
                case 2: return new Vector2(Random.Range(left, right), bottom);
                default:return new Vector2(Random.Range(left, right), top);
            }
        }

        static Vector2 ClampTo(Vector2 min, Vector2 max, Vector2 p)
        {
            return new Vector2(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y));
        }
    }
}
