using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class WaterFillManager : MonoBehaviour
{
    // ============================================================
    // SHADER PROPERTY
    // ============================================================

    [Header("Shader Property")]
    [SerializeField] private string fillProperty = "_FillAmount";


    // ============================================================
    // FILL VALUES
    // ============================================================

    [Header("Fill Value List")]
    [SerializeField]
    private List<float> fillValues = new List<float>
    {
        0f,
        0.21f,
        0.42f,
        0.63f,
        0.84f,
        1f
    };


    // ============================================================
    // WATER RENDERERS
    // ============================================================

    [Header("========== WATER RENDERERS ==========")]

    [SerializeField] private Renderer waterRenderer1;
    [SerializeField] private Renderer waterRenderer2;
    [SerializeField] private Renderer waterRenderer3;

    [SerializeField] private int materialIndex1 = 0;
    [SerializeField] private int materialIndex2 = 0;
    [SerializeField] private int materialIndex3 = 0;

    private Material _waterMaterial1;
    private Material _waterMaterial2;
    private Material _waterMaterial3;


    // ============================================================
    // QUESTION 1
    // ============================================================

    [Header("========== QUESTION 1 ==========")]

    [Header("Question 1 Buttons")]

    [SerializeField] private Button plusButton1;
    [SerializeField] private Button minusButton1;
    [SerializeField] private Button checkButton1;


    [Header("Question 1 Correct Answer")]

    [SerializeField] private int correctIndex1 = 3;


    [Header("Question 1 TMP")]

    [SerializeField] private TMP_Text countText1;


    [Header("Question 1 Starting Value")]

    [SerializeField] private int startingIndex1 = 0;


    [Header("Question 1 Result Images")]

    [Tooltip("Shown when Question 1 answer is correct.")]
    [SerializeField] private GameObject correctImage1;

    [Tooltip("Shown when Question 1 answer is wrong.")]
    [SerializeField] private GameObject wrongImage1;


    // ============================================================
    // QUESTION 2
    // ============================================================

    [Header("========== QUESTION 2 ==========")]

    [Header("Question 2 Buttons")]

    [SerializeField] private Button plusButton2;
    [SerializeField] private Button minusButton2;
    [SerializeField] private Button checkButton2;


    [Header("Question 2 Correct Answer")]

    [SerializeField] private int correctIndex2 = 3;


    [Header("Question 2 TMP")]

    [SerializeField] private TMP_Text countText2;


    [Header("Question 2 Starting Value")]

    [SerializeField] private int startingIndex2 = 0;


    [Header("Question 2 Result Images")]

    [Tooltip("Shown when Question 2 answer is correct.")]
    [SerializeField] private GameObject correctImage2;

    [Tooltip("Shown when Question 2 answer is wrong.")]
    [SerializeField] private GameObject wrongImage2;


    // ============================================================
    // QUESTION 3
    // ============================================================

    [Header("========== QUESTION 3 ==========")]

    [Header("Question 3 Buttons")]

    [SerializeField] private Button plusButton3;
    [SerializeField] private Button minusButton3;
    [SerializeField] private Button checkButton3;


    [Header("Question 3 Correct Answer")]

    [SerializeField] private int correctIndex3 = 3;


    [Header("Question 3 TMP")]

    [SerializeField] private TMP_Text countText3;


    [Header("Question 3 Starting Value")]

    [SerializeField] private int startingIndex3 = 0;


    [Header("Question 3 Result Images")]

    [Tooltip("Shown when Question 3 answer is correct.")]
    [SerializeField] private GameObject correctImage3;

    [Tooltip("Shown when Question 3 answer is wrong.")]
    [SerializeField] private GameObject wrongImage3;


    // ============================================================
    // ALL QUESTIONS COMPLETE
    // ============================================================

    [Header("========== ALL 3 QUESTIONS COMPLETE ==========")]

    [Tooltip(
        "Invoked only after Question 1, Question 2 and Question 3 " +
        "are all answered correctly."
    )]
    [SerializeField] private UnityEvent onAllQuestionsCorrect;


    // ============================================================
    // CLAMP RANGE
    // ============================================================

    [Header("Clamp Range")]

    [SerializeField] private float minFill = 0f;
    [SerializeField] private float maxFill = 1f;


    // ============================================================
    // QUESTION 1 RUNTIME
    // ============================================================

    private float _currentFill1;
    private int _currentIndex1;
    private bool _question1Completed = false;


    // ============================================================
    // QUESTION 2 RUNTIME
    // ============================================================

    private float _currentFill2;
    private int _currentIndex2;
    private bool _question2Completed = false;


    // ============================================================
    // QUESTION 3 RUNTIME
    // ============================================================

    private float _currentFill3;
    private int _currentIndex3;
    private bool _question3Completed = false;


    // ============================================================
    // ALL COMPLETE
    // ============================================================

    private bool _allQuestionsCompleted = false;


    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        ResolveWaterMaterials();

        RegisterButtonListeners();

        InitializeUI();
    }


    private void Start()
    {
        InitializeQuestion1();
        InitializeQuestion2();
        InitializeQuestion3();

        CheckAllQuestionsCompleted();
    }


    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }


    // ============================================================
    // WATER MATERIAL SETUP
    // ============================================================

    private void ResolveWaterMaterials()
    {
        _waterMaterial1 = GetMaterialFromRenderer(
            waterRenderer1,
            materialIndex1,
            "Water Renderer 1"
        );


        _waterMaterial2 = GetMaterialFromRenderer(
            waterRenderer2,
            materialIndex2,
            "Water Renderer 2"
        );


        _waterMaterial3 = GetMaterialFromRenderer(
            waterRenderer3,
            materialIndex3,
            "Water Renderer 3"
        );
    }


    private Material GetMaterialFromRenderer(
        Renderer targetRenderer,
        int materialIndex,
        string rendererName)
    {
        if (targetRenderer == null)
        {
            Debug.LogError(
                $"[WaterFillManager] {rendererName} is not assigned."
            );

            return null;
        }


        Material[] materials = targetRenderer.materials;


        if (materialIndex < 0 ||
            materialIndex >= materials.Length)
        {
            Debug.LogError(
                $"[WaterFillManager] {rendererName}: " +
                $"Material Index {materialIndex} is out of range."
            );

            return null;
        }


        return materials[materialIndex];
    }


    // ============================================================
    // INITIAL UI
    // ============================================================

    private void InitializeUI()
    {
        HideCheckButton(checkButton1);
        HideCheckButton(checkButton2);
        HideCheckButton(checkButton3);


        EnablePlusMinus(
            plusButton1,
            minusButton1
        );


        EnablePlusMinus(
            plusButton2,
            minusButton2
        );


        EnablePlusMinus(
            plusButton3,
            minusButton3
        );


        // Hide all result images initially.

        HideResultImages(
            correctImage1,
            wrongImage1
        );


        HideResultImages(
            correctImage2,
            wrongImage2
        );


        HideResultImages(
            correctImage3,
            wrongImage3
        );
    }


    // ============================================================
    // QUESTION 1 INITIALIZE
    // ============================================================

    private void InitializeQuestion1()
    {
        if (fillValues == null ||
            fillValues.Count == 0)
        {
            _currentIndex1 = 0;
            _currentFill1 = minFill;

            UpdateCounter(
                countText1,
                _currentIndex1
            );

            return;
        }


        _currentIndex1 = Mathf.Clamp(
            startingIndex1,
            0,
            fillValues.Count - 1
        );


        _currentFill1 = Mathf.Clamp(
            fillValues[_currentIndex1],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial1,
            _currentFill1
        );


        UpdateCounter(
            countText1,
            _currentIndex1
        );


        HideCheckButton(checkButton1);


        HideResultImages(
            correctImage1,
            wrongImage1
        );
    }


    // ============================================================
    // QUESTION 2 INITIALIZE
    // ============================================================

    private void InitializeQuestion2()
    {
        if (fillValues == null ||
            fillValues.Count == 0)
        {
            _currentIndex2 = 0;
            _currentFill2 = minFill;

            UpdateCounter(
                countText2,
                _currentIndex2
            );

            return;
        }


        _currentIndex2 = Mathf.Clamp(
            startingIndex2,
            0,
            fillValues.Count - 1
        );


        _currentFill2 = Mathf.Clamp(
            fillValues[_currentIndex2],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial2,
            _currentFill2
        );


        UpdateCounter(
            countText2,
            _currentIndex2
        );


        HideCheckButton(checkButton2);


        HideResultImages(
            correctImage2,
            wrongImage2
        );
    }


    // ============================================================
    // QUESTION 3 INITIALIZE
    // ============================================================

    private void InitializeQuestion3()
    {
        if (fillValues == null ||
            fillValues.Count == 0)
        {
            _currentIndex3 = 0;
            _currentFill3 = minFill;

            UpdateCounter(
                countText3,
                _currentIndex3
            );

            return;
        }


        _currentIndex3 = Mathf.Clamp(
            startingIndex3,
            0,
            fillValues.Count - 1
        );


        _currentFill3 = Mathf.Clamp(
            fillValues[_currentIndex3],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial3,
            _currentFill3
        );


        UpdateCounter(
            countText3,
            _currentIndex3
        );


        HideCheckButton(checkButton3);


        HideResultImages(
            correctImage3,
            wrongImage3
        );
    }


    // ============================================================
    // BUTTON REGISTRATION
    // ============================================================

    private void RegisterButtonListeners()
    {
        // QUESTION 1

        if (plusButton1 != null)
            plusButton1.onClick.AddListener(OnPlus1);

        if (minusButton1 != null)
            minusButton1.onClick.AddListener(OnMinus1);

        if (checkButton1 != null)
            checkButton1.onClick.AddListener(OnCheck1);


        // QUESTION 2

        if (plusButton2 != null)
            plusButton2.onClick.AddListener(OnPlus2);

        if (minusButton2 != null)
            minusButton2.onClick.AddListener(OnMinus2);

        if (checkButton2 != null)
            checkButton2.onClick.AddListener(OnCheck2);


        // QUESTION 3

        if (plusButton3 != null)
            plusButton3.onClick.AddListener(OnPlus3);

        if (minusButton3 != null)
            minusButton3.onClick.AddListener(OnMinus3);

        if (checkButton3 != null)
            checkButton3.onClick.AddListener(OnCheck3);
    }


    private void UnregisterButtonListeners()
    {
        if (plusButton1 != null)
            plusButton1.onClick.RemoveListener(OnPlus1);

        if (minusButton1 != null)
            minusButton1.onClick.RemoveListener(OnMinus1);

        if (checkButton1 != null)
            checkButton1.onClick.RemoveListener(OnCheck1);


        if (plusButton2 != null)
            plusButton2.onClick.RemoveListener(OnPlus2);

        if (minusButton2 != null)
            minusButton2.onClick.RemoveListener(OnMinus2);

        if (checkButton2 != null)
            checkButton2.onClick.RemoveListener(OnCheck2);


        if (plusButton3 != null)
            plusButton3.onClick.RemoveListener(OnPlus3);

        if (minusButton3 != null)
            minusButton3.onClick.RemoveListener(OnMinus3);

        if (checkButton3 != null)
            checkButton3.onClick.RemoveListener(OnCheck3);
    }


    // ============================================================
    // QUESTION 1 PLUS
    // ============================================================

    private void OnPlus1()
    {
        if (_question1Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int nextIndex = Mathf.Clamp(
            _currentIndex1 + 1,
            0,
            fillValues.Count - 1
        );


        if (nextIndex == _currentIndex1)
        {
            ShowCheckButton(checkButton1);
            return;
        }


        SetQuestion1Index(nextIndex);

        ShowCheckButton(checkButton1);


        // New value means previous result is no longer valid.
        HideResultImages(
            correctImage1,
            wrongImage1
        );
    }


    // ============================================================
    // QUESTION 1 MINUS
    // ============================================================

    private void OnMinus1()
    {
        if (_question1Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int previousIndex = Mathf.Clamp(
            _currentIndex1 - 1,
            0,
            fillValues.Count - 1
        );


        if (previousIndex == _currentIndex1)
        {
            ShowCheckButton(checkButton1);
            return;
        }


        SetQuestion1Index(previousIndex);

        ShowCheckButton(checkButton1);


        HideResultImages(
            correctImage1,
            wrongImage1
        );
    }


    // ============================================================
    // QUESTION 1 CHECK
    // ============================================================

    private void OnCheck1()
    {
        if (_question1Completed)
            return;


        if (_currentIndex1 == correctIndex1)
        {
            _question1Completed = true;


            // Correct image ON.
            ShowCorrectImage(
                correctImage1,
                wrongImage1
            );


            HideCheckButton(checkButton1);


            DisablePlusMinus(
                plusButton1,
                minusButton1
            );


            Debug.Log(
                "[WaterFillManager] QUESTION 1 CORRECT."
            );


            CheckAllQuestionsCompleted();
        }
        else
        {
            // Wrong image ON.
            ShowWrongImage(
                correctImage1,
                wrongImage1
            );


            ShowCheckButton(checkButton1);


            Debug.Log(
                "[WaterFillManager] QUESTION 1 WRONG."
            );
        }
    }


    // ============================================================
    // QUESTION 2 PLUS
    // ============================================================

    private void OnPlus2()
    {
        if (_question2Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int nextIndex = Mathf.Clamp(
            _currentIndex2 + 1,
            0,
            fillValues.Count - 1
        );


        if (nextIndex == _currentIndex2)
        {
            ShowCheckButton(checkButton2);
            return;
        }


        SetQuestion2Index(nextIndex);

        ShowCheckButton(checkButton2);


        HideResultImages(
            correctImage2,
            wrongImage2
        );
    }


    // ============================================================
    // QUESTION 2 MINUS
    // ============================================================

    private void OnMinus2()
    {
        if (_question2Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int previousIndex = Mathf.Clamp(
            _currentIndex2 - 1,
            0,
            fillValues.Count - 1
        );


        if (previousIndex == _currentIndex2)
        {
            ShowCheckButton(checkButton2);
            return;
        }


        SetQuestion2Index(previousIndex);

        ShowCheckButton(checkButton2);


        HideResultImages(
            correctImage2,
            wrongImage2
        );
    }


    // ============================================================
    // QUESTION 2 CHECK
    // ============================================================

    private void OnCheck2()
    {
        if (_question2Completed)
            return;


        if (_currentIndex2 == correctIndex2)
        {
            _question2Completed = true;


            ShowCorrectImage(
                correctImage2,
                wrongImage2
            );


            HideCheckButton(checkButton2);


            DisablePlusMinus(
                plusButton2,
                minusButton2
            );


            Debug.Log(
                "[WaterFillManager] QUESTION 2 CORRECT."
            );


            CheckAllQuestionsCompleted();
        }
        else
        {
            ShowWrongImage(
                correctImage2,
                wrongImage2
            );


            ShowCheckButton(checkButton2);


            Debug.Log(
                "[WaterFillManager] QUESTION 2 WRONG."
            );
        }
    }


    // ============================================================
    // QUESTION 3 PLUS
    // ============================================================

    private void OnPlus3()
    {
        if (_question3Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int nextIndex = Mathf.Clamp(
            _currentIndex3 + 1,
            0,
            fillValues.Count - 1
        );


        if (nextIndex == _currentIndex3)
        {
            ShowCheckButton(checkButton3);
            return;
        }


        SetQuestion3Index(nextIndex);

        ShowCheckButton(checkButton3);


        HideResultImages(
            correctImage3,
            wrongImage3
        );
    }


    // ============================================================
    // QUESTION 3 MINUS
    // ============================================================

    private void OnMinus3()
    {
        if (_question3Completed)
            return;


        if (fillValues == null ||
            fillValues.Count == 0)
            return;


        int previousIndex = Mathf.Clamp(
            _currentIndex3 - 1,
            0,
            fillValues.Count - 1
        );


        if (previousIndex == _currentIndex3)
        {
            ShowCheckButton(checkButton3);
            return;
        }


        SetQuestion3Index(previousIndex);

        ShowCheckButton(checkButton3);


        HideResultImages(
            correctImage3,
            wrongImage3
        );
    }


    // ============================================================
    // QUESTION 3 CHECK
    // ============================================================

    private void OnCheck3()
    {
        if (_question3Completed)
            return;


        if (_currentIndex3 == correctIndex3)
        {
            _question3Completed = true;


            ShowCorrectImage(
                correctImage3,
                wrongImage3
            );


            HideCheckButton(checkButton3);


            DisablePlusMinus(
                plusButton3,
                minusButton3
            );


            Debug.Log(
                "[WaterFillManager] QUESTION 3 CORRECT."
            );


            CheckAllQuestionsCompleted();
        }
        else
        {
            ShowWrongImage(
                correctImage3,
                wrongImage3
            );


            ShowCheckButton(checkButton3);


            Debug.Log(
                "[WaterFillManager] QUESTION 3 WRONG."
            );
        }
    }


    // ============================================================
    // CHECK ALL QUESTIONS
    // ============================================================

    private void CheckAllQuestionsCompleted()
    {
        Debug.Log(
            $"[WaterFillManager] " +
            $"Q1 = {_question1Completed}, " +
            $"Q2 = {_question2Completed}, " +
            $"Q3 = {_question3Completed}"
        );


        if (_allQuestionsCompleted)
            return;


        if (!_question1Completed ||
            !_question2Completed ||
            !_question3Completed)
        {
            Debug.Log(
                "[WaterFillManager] " +
                "Not all questions are completed yet."
            );

            return;
        }


        _allQuestionsCompleted = true;


        Debug.Log(
            "[WaterFillManager] ALL 3 QUESTIONS CORRECT!"
        );


        if (onAllQuestionsCorrect != null)
        {
            onAllQuestionsCorrect.Invoke();


            Debug.Log(
                "[WaterFillManager] " +
                "On All Questions Correct Event INVOKED."
            );
        }
        else
        {
            Debug.LogError(
                "[WaterFillManager] " +
                "On All Questions Correct UnityEvent is NULL."
            );
        }
    }


    // ============================================================
    // SET QUESTION 1 INDEX
    // ============================================================

    private void SetQuestion1Index(int index)
    {
        if (_question1Completed)
            return;


        index = Mathf.Clamp(
            index,
            0,
            fillValues.Count - 1
        );


        _currentIndex1 = index;


        _currentFill1 = Mathf.Clamp(
            fillValues[index],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial1,
            _currentFill1
        );


        UpdateCounter(
            countText1,
            _currentIndex1
        );
    }


    // ============================================================
    // SET QUESTION 2 INDEX
    // ============================================================

    private void SetQuestion2Index(int index)
    {
        if (_question2Completed)
            return;


        index = Mathf.Clamp(
            index,
            0,
            fillValues.Count - 1
        );


        _currentIndex2 = index;


        _currentFill2 = Mathf.Clamp(
            fillValues[index],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial2,
            _currentFill2
        );


        UpdateCounter(
            countText2,
            _currentIndex2
        );
    }


    // ============================================================
    // SET QUESTION 3 INDEX
    // ============================================================

    private void SetQuestion3Index(int index)
    {
        if (_question3Completed)
            return;


        index = Mathf.Clamp(
            index,
            0,
            fillValues.Count - 1
        );


        _currentIndex3 = index;


        _currentFill3 = Mathf.Clamp(
            fillValues[index],
            minFill,
            maxFill
        );


        ApplyFill(
            _waterMaterial3,
            _currentFill3
        );


        UpdateCounter(
            countText3,
            _currentIndex3
        );
    }


    // ============================================================
    // APPLY FILL
    // ============================================================

    private void ApplyFill(
        Material targetMaterial,
        float value)
    {
        if (targetMaterial == null)
            return;


        targetMaterial.SetFloat(
            fillProperty,
            value
        );
    }


    // ============================================================
    // TMP
    // ============================================================

    private void UpdateCounter(
        TMP_Text text,
        int value)
    {
        if (text == null)
            return;


        text.text = value.ToString();
    }


    // ============================================================
    // CHECK BUTTON
    // ============================================================

    private void ShowCheckButton(
        Button button)
    {
        if (button == null)
            return;


        button.gameObject.SetActive(true);
    }


    private void HideCheckButton(
        Button button)
    {
        if (button == null)
            return;


        button.gameObject.SetActive(false);
    }


    // ============================================================
    // RESULT IMAGES
    // ============================================================

    private void HideResultImages(
        GameObject correctImage,
        GameObject wrongImage)
    {
        if (correctImage != null)
        {
            correctImage.SetActive(false);
        }


        if (wrongImage != null)
        {
            wrongImage.SetActive(false);
        }
    }


    private void ShowCorrectImage(
        GameObject correctImage,
        GameObject wrongImage)
    {
        // Wrong image OFF.
        if (wrongImage != null)
        {
            wrongImage.SetActive(false);
        }


        // Correct image ON.
        if (correctImage != null)
        {
            correctImage.SetActive(true);
        }
    }


    private void ShowWrongImage(
        GameObject correctImage,
        GameObject wrongImage)
    {
        // Correct image OFF.
        if (correctImage != null)
        {
            correctImage.SetActive(false);
        }


        // Wrong image ON.
        if (wrongImage != null)
        {
            wrongImage.SetActive(true);
        }
    }


    // ============================================================
    // PLUS / MINUS
    // ============================================================

    private void EnablePlusMinus(
        Button plus,
        Button minus)
    {
        if (plus != null)
        {
            plus.interactable = true;
        }


        if (minus != null)
        {
            minus.interactable = true;
        }
    }


    private void DisablePlusMinus(
        Button plus,
        Button minus)
    {
        if (plus != null)
        {
            plus.interactable = false;
        }


        if (minus != null)
        {
            minus.interactable = false;
        }
    }


    // ============================================================
    // PUBLIC STATUS
    // ============================================================

    public bool IsQuestion1Completed()
    {
        return _question1Completed;
    }


    public bool IsQuestion2Completed()
    {
        return _question2Completed;
    }


    public bool IsQuestion3Completed()
    {
        return _question3Completed;
    }


    public bool AreAllQuestionsCompleted()
    {
        return _allQuestionsCompleted;
    }


    public int GetCurrentIndex1()
    {
        return _currentIndex1;
    }


    public int GetCurrentIndex2()
    {
        return _currentIndex2;
    }


    public int GetCurrentIndex3()
    {
        return _currentIndex3;
    }


    // ============================================================
    // RESET ALL
    // ============================================================

    public void ResetAllQuestions()
    {
        _question1Completed = false;
        _question2Completed = false;
        _question3Completed = false;

        _allQuestionsCompleted = false;


        EnablePlusMinus(
            plusButton1,
            minusButton1
        );


        EnablePlusMinus(
            plusButton2,
            minusButton2
        );


        EnablePlusMinus(
            plusButton3,
            minusButton3
        );


        HideCheckButton(checkButton1);
        HideCheckButton(checkButton2);
        HideCheckButton(checkButton3);


        HideResultImages(
            correctImage1,
            wrongImage1
        );


        HideResultImages(
            correctImage2,
            wrongImage2
        );


        HideResultImages(
            correctImage3,
            wrongImage3
        );


        InitializeQuestion1();
        InitializeQuestion2();
        InitializeQuestion3();


        Debug.Log(
            "[WaterFillManager] All questions reset."
        );
    }
}