using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string text;

    
}

[System.Serializable]
public class ImageDialogueData
{
    public Sprite image;
    public List<DialogueLine> dialogueLines;
}

public class ImageTextSequencePlayer : MonoBehaviour
{
    
    [Header("UI References")]
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Data")]
    public List<ImageDialogueData> sequenceData;

    [Header("Typewriter Settings")]
    public float characterInterval = 0.04f;
    public AudioSource typeSound;

    private int currentImageIndex = 0;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    public GameObject sceneManager;
    void Start()
    {
        ShowCurrentDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 快速显示完整文字
                StopCoroutine(typingCoroutine);
                dialogueText.text = GetCurrentLine().text;
                isTyping = false;
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    void AdvanceDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < sequenceData[currentImageIndex].dialogueLines.Count)
        {
            ShowCurrentDialogue();
        }
        else
        {
            currentImageIndex++;
            currentDialogueIndex = 0;

            if (currentImageIndex < sequenceData.Count)
            {
                ShowCurrentDialogue();
            }
            else
            {
                sceneManager.SetActive(true);
            }
        }
    }

    void ShowCurrentDialogue()
    {
        var line = GetCurrentLine();

        nameText.text = line.speakerName;
        backgroundImage.sprite = sequenceData[currentImageIndex].image;

        StartTypingText(line.text);
    }

    DialogueLine GetCurrentLine()
    {
        return sequenceData[currentImageIndex].dialogueLines[currentDialogueIndex];
    }

    void StartTypingText(string fullText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";
        int count = 0;
        foreach (char c in fullText)
        {
            dialogueText.text += c;

            if (typeSound != null && typeSound.clip != null && count % 2 == 0)
            {
                typeSound.PlayOneShot(typeSound.clip);
            }

            count++;
            yield return new WaitForSeconds(characterInterval);
        }

        isTyping = false;
    }


    // void EndSequence()
    // {
    //     dialogueText.text = "";
    //     nameText.text = "";
    //     backgroundImage.enabled = false;
    //     Debug.Log("Dialogue sequence complete.");
    //     sceneManager.SetActive(true);
    // }
}
