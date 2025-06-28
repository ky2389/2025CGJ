using UnityEngine;

public class StarFlow : MonoBehaviour
{
    public int lineCount = 50;
    public Vector2 speedRange = new Vector2(0.5f, 2f);
    public Vector2 lengthRange = new Vector2(1f, 4f);
    public Material lineMaterial;
    public Gradient[] colorPalettes; // 多种颜色组合

    private class StarLine
    {
        public LineRenderer lr;
        public float speed;
        public float length;
        public Vector3 startPos;
        public Gradient color;
    }

    private StarLine[] starLines;

    void Start()
    {
        starLines = new StarLine[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            GameObject go = new GameObject("StarLine_" + i);
            go.transform.parent = transform;

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = lineMaterial;
            lr.widthMultiplier = 0.03f;
            lr.numCapVertices = 0;
            lr.useWorldSpace = true;

            StarLine star = new StarLine();
            star.lr = lr;
            ResetLine(star);
            starLines[i] = star;
        }
    }

    void Update()
    {
        foreach (var line in starLines)
        {
            line.startPos.x += line.speed * Time.deltaTime;

            Vector3 endPos = line.startPos + new Vector3(line.length, 0, 0);
            line.lr.SetPosition(0, line.startPos);
            line.lr.SetPosition(1, endPos);

            if (line.startPos.x > Camera.main.orthographicSize * Camera.main.aspect + 1f)
            {
                ResetLine(line);
            }
        }
    }

    void ResetLine(StarLine line)
    {
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;

        line.startPos = new Vector3(-screenWidth / 2f - Random.Range(0f, 2f), Random.Range(-screenHeight / 2f, screenHeight / 2f), 0);
        line.length = Random.Range(lengthRange.x, lengthRange.y);
        line.speed = Random.Range(speedRange.x, speedRange.y);
        line.color = colorPalettes[Random.Range(0, colorPalettes.Length)];

        line.lr.colorGradient = line.color;
    }
}
