using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachineReel : MonoBehaviour
{
    [Header("Reel Settings")]
    [SerializeField] private float spinSpeed = 30f, spinningTime = 1f, slowDownTime = 2f;
    [SerializeField] private float symbolHeight = 1f, verticalSpacing = 0.2f;
    [SerializeField] private Transform symbolsParent;
    [SerializeField] private int reelIndex = 0;

    [Header("Symbol Settings")]
    [SerializeField] private List<GameObject> symbolPrefabs;
    [SerializeField] private int numberOfVisibleSymbols = 3;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [Header("Alignment")]
    [SerializeField] private float centerPositionY = 0f;
    [SerializeField] private bool alignToGridOnStart = true, forceDownwardStop = true;

    private List<Transform> symbols = new List<Transform>();
    private List<int> symbolIndices = new List<int>();
    private bool isSpinning = false;
    private float currentSpeed = 0f, totalSymbolSize;
    private int targetSymbolIndex = -1, lastCenterSymbolIndex = -1;

    void Awake()
    {
        totalSymbolSize = symbolHeight + verticalSpacing;

        // Collect existing symbols
        foreach (Transform child in symbolsParent)
        {
            symbols.Add(child);
            symbolIndices.Add(GetSymbolIndex(child.gameObject));
        }
    }

    void Start()
    {
        symbolsParent.localPosition = new Vector3(0,0,0);

        // Align symbols to grid
        if (alignToGridOnStart && symbols.Count > 0)
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] != null)
                    symbols[i].localPosition = new Vector3(symbols[i].localPosition.x, i * totalSymbolSize, symbols[i].localPosition.z);
            }
        }
    }

    void Update()
    {
        if (!isSpinning) return;

        // Move symbols down
        symbolsParent.Translate(Vector3.down * currentSpeed * Time.deltaTime);

        // Reposition symbols that go out of view
        float totalHeight = totalSymbolSize * symbols.Count;
        for (int i = 0; i < symbols.Count; i++)
        {
            if (symbols[i] == null) continue;

            // If symbol goes below the bottom of the reel
            if (symbols[i].localPosition.y < -totalHeight / 2)
            {
                // Move it to the top
                symbols[i].localPosition += new Vector3(0, totalHeight, 0);
            }
        }

        // Reset parent position when it moves too far
        if (symbolsParent.localPosition.y < -totalSymbolSize)
        {
            float offset = totalSymbolSize * Mathf.Floor(symbolsParent.localPosition.y / -totalSymbolSize);
            symbolsParent.localPosition += new Vector3(0, offset, 0);

            foreach (Transform symbol in symbols)
            {
                if (symbol != null)
                    symbol.localPosition -= new Vector3(0, offset, 0);
            }
        }
    }

    public void Spin(int targetIndex = -1)
    {
        if (!isSpinning)
        {
            Start();
            targetSymbolIndex = targetIndex;
            if (debugMode)
                Debug.Log($"Reel {reelIndex}: Starting spin with target {targetIndex}");

            StartCoroutine(SpinRoutine());
        }
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        currentSpeed = 0f;

        // Accelerate to max speed
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0f, spinSpeed, t / 0.3f);
            yield return null;
        }

        // Spin at max speed
        yield return new WaitForSeconds(spinningTime);

        // Determine landing symbol (random if not specified)
        int landingPosition;
        if (targetSymbolIndex >= 0)
        {
            landingPosition = targetSymbolIndex;
        }
        else
        {
            // Get a different random value for each reel
            float randomValue = Random.value + reelIndex * 0.37f;
            randomValue = randomValue % 1.0f;
            landingPosition = Mathf.FloorToInt(randomValue * symbolPrefabs.Count);
            landingPosition = 1;
        }

        if (debugMode)
            Debug.Log($"Reel {reelIndex}: Landing position chosen: {landingPosition}");
        Vector3 startPosition = symbolsParent.localPosition;
        Vector3 targetPosition = Vector3.zero; // Target is (0,0,0)
        // Slow down gradually
        t = 0f;
        float initialSpeed = currentSpeed;
        while (t < slowDownTime)
        {
            t += Time.deltaTime;
            float percentageSpeed = 1f - (1f - t / slowDownTime) * (1f - t / slowDownTime) * (1f - t / slowDownTime);
            currentSpeed = Mathf.Lerp(initialSpeed, 0f, percentageSpeed);


            float percentageParent= 1f - (1f - t / slowDownTime) * (1f - t / slowDownTime) * (1f - t / slowDownTime)* (1f - t / slowDownTime);

            symbolsParent.localPosition = Vector3.Lerp(startPosition, targetPosition, percentageParent);
            yield return null;
        }

        // Snap to grid and apply target symbol
        SnapToNearestSymbol(landingPosition);
        currentSpeed = 0f;
        isSpinning = false;
        symbolsParent.localPosition = new Vector3(0, 0, 0);


    }

    private void SnapToNearestSymbol(int targetIndex)
    {

        lastCenterSymbolIndex = targetIndex;
        for (int i = 0; i < symbols.Count; i++)
        {
            if (i == targetIndex)
            {
                symbols[i].localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                symbols[i].localPosition = new Vector3(0, 10000, 0);
            }

        }
    }


    private int GetSymbolIndex(GameObject symbolObj)
    {
        if (symbolObj == null) return 0;

        if (symbolObj.name.StartsWith("Symbol_") && symbolObj.name.Length > 7)
        {
            string indexStr = symbolObj.name.Substring(7).Split('_')[0];
            if (int.TryParse(indexStr, out int index) &&
                index >= 0 && index < symbolPrefabs.Count)
                return index;
        }

        return 0;
    }

    // Public methods
    public void SpinWithRandomResult() => Spin(-1);

    public void SpinWithResult(int symbolIndex)
    {
        if (symbolPrefabs.Count > 0)
            Spin(Mathf.Clamp(symbolIndex, 0, symbolPrefabs.Count - 1));
    }

    public int GetCenterSymbolIndex()
    {
        return lastCenterSymbolIndex;

    }

}