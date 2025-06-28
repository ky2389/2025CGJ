using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class FolderFramePlayer : MonoBehaviour
{
    public string resourcesFolderPath = "VideoFrames"; // 放在 Resources/VideoFrames/
    public Image targetImage;        // 用于播放序列的UI Image
    public float frameRate = 10f;

    private Sprite[] frames;
    private int currentFrame = 0;
    private bool isReversing = false;
    private float timer = 0f;

    void Start()
    {
        LoadFrames();
    }

    void LoadFrames()
    {
        frames = Resources.LoadAll<Sprite>(resourcesFolderPath);

        // 排序（按名字中的数字排序）
        frames = frames.OrderBy(s => ExtractNumber(s.name)).ToArray();
    }

    int ExtractNumber(string name)
    {
        string digits = new string(name.Where(char.IsDigit).ToArray());
        int.TryParse(digits, out int number);
        return number;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || targetImage == null)
            return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;

            if (frames[currentFrame] != null)
                targetImage.sprite = frames[currentFrame];

            if (!isReversing)
            {
                currentFrame++;
                if (currentFrame >= frames.Length)
                {
                    currentFrame = frames.Length - 1;
                    isReversing = true;
                }
            }
            else
            {
                currentFrame--;
                if (currentFrame < 0)
                {
                    currentFrame = 0;
                    isReversing = false;
                }
            }
        }
    }
}