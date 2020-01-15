using UnityEngine;

public class Tile
{
    GameObject gameObject;

    public bool IsClicked { get; set; }

    public bool IsFlagged { get; set; }

    public bool HasBomb { get; set; }

    public int AdjacentBombs { get; set; }

    public static string[] Colors = new string[] { "#1DABCA", "#6D8A25", "#C44073", "#2E62C0", "#AA2020", "#1E6D32", "#651E6D", "#FF8000", "#000000" };

    public Tile(GameObject obj)
    {
        gameObject = obj;
    }

    public void SetPosition(Vector2 position)
    {
        if (gameObject == null)
            return;

        gameObject.transform.localPosition = position;
    }

    public ClickResult OnClick()
    {
        if (IsFlagged || IsClicked)
            return ClickResult.AlreadyClicked;

        IsClicked = true;

        if (HasBomb)
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            renderer.color = Color.red;
            return ClickResult.Mine;
        }

        if(AdjacentBombs > 0)
        {
            DisplayText();
            return ClickResult.Normal;
        }

        DisplayClearBackground();
        return ClickResult.Clear;
    }

    void DisplayClearBackground()
    {
        var renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.75f, 0.75f, 0.75f);
    }

    void DisplayText()
    {
        if (AdjacentBombs <= 0 || AdjacentBombs >= 9)
            return;

        DisplayClearBackground();

        var textObj = gameObject.transform.Find("Text");
        var text = textObj.GetComponent<TextMesh>();
        text.text = $"{AdjacentBombs}"; 
        ColorUtility.TryParseHtmlString(Colors[(AdjacentBombs - 1)], out var color);
        text.color = color;
    }

    public ClickResult OnFlag()
    {
        if (IsClicked)
            return ClickResult.AlreadyClicked;

        IsFlagged = !IsFlagged;

        var renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.color = IsFlagged ? Color.blue : Color.white;

        return IsFlagged ? ClickResult.Flagged : ClickResult.UnFlagged;
    }
}
