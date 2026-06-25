using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameLogicHandler : MonoBehaviour
{
    #region STATIC_VARIABLES
    public static GameLogicHandler instance { get; set; }
    #endregion
    #region PUBLIC_VARIABLES
    public int gameSize = 2;
    #endregion
    #region PRIVATE_VARIABLES
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject cardList;
    [SerializeField] private Sprite cardBack;
    [SerializeField] private Sprite[] sprite_Cards;
    private Card[] cards;

    [SerializeField] private GameObject panel_Gameplay;
    [SerializeField] private GameObject panel_congratulations;
    [SerializeField] private GameObject panel_Score;
    [SerializeField] private GameObject panel_Timer;
    [SerializeField] private Card spritePreload;
    [SerializeField] private TMP_Text txt_Score;
    [SerializeField] private TMP_Text txt_SizeLabel;
    [SerializeField] private Slider slider_GameSize;
    [SerializeField] private TMP_Text txt_timeLabel;
    [SerializeField] private Canvas canvasGamePlay;
    [SerializeField] private GameObject obj_GiveUpCanvas;
    private float time = 0f;
    private int score = 0;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    #endregion

    #region UNITY_METHODS
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        gameStart = false;
    }
    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            txt_timeLabel.text = "Time: " + (int)time + "s";
        }
    }

    #endregion

    #region PRIVATE_METHODS
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprite_Cards.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
    }
    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd];
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        RectTransform panelsize = panel_Gameplay.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject cardObj;

                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    cardObj = cards[index].gameObject;
                }
                else
                {

                    cardObj = Instantiate(prefab);
                    cardObj.transform.parent = cardList.transform;
                    int index = i * gameSize + j;
                    cards[index] = cardObj.GetComponent<Card>();
                    cards[index].ID = index;
                    cardObj.transform.localScale = new Vector3(scale, scale);
                }
                cardObj.transform.localPosition = new Vector3(curX, curY, 0);
                Debug.Log("X pos " + curX);
                Debug.Log("Y pos " + curY);
                curX += xInc;

            }
            curY += yInc;
        }

    }
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }
    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.7f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        for (i = 0; i < cards.Length / 2; i++)
        {

            int value = Random.Range(0, sprite_Cards.Length - 1);
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprite_Cards.Length;
            }
            selectedID[i] = value;
        }


        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }

        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }
    private void CheckGameWin()
    {
        
        if (cardLeft == 0)
        {
            panel_congratulations.SetActive(true);
            EndGame();
        }
    }
   
    private void EndGame()
    {
        gameStart = false;
        canvasGamePlay.enabled = false;
    }
    #endregion

    #region PUBLIC_METHODS
    public void StartCardGame()
    {
        if (gameStart)
            return;
        gameStart = true;
        canvasGamePlay.enabled = true;
        panel_congratulations.SetActive(false);
        panel_Timer.SetActive(true);
        panel_Score.SetActive(true);
        SetGamePanel();
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
        score = 0;
    }
    public void SetGameSize()
    {
        gameSize = (int)slider_GameSize.value;
        txt_SizeLabel.text = gameSize + " X " + gameSize;
    }

    public Sprite GetSprite(int spriteId)
    {
        return sprite_Cards[spriteId];
    }
    public Sprite CardBack()
    {
        return cardBack;
    }
    public void GiveUpButtonCLicked()
    {
        if (!gameStart)
            return;
        obj_GiveUpCanvas.SetActive(true);
    }
    public void GiveUp()
    {
        EndGame();
    }

    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }

    public void cardClicked(int spriteId, int cardId)
    {

        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                score++;
                txt_Score.text = score.ToString();
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {

                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }
    #endregion
}
