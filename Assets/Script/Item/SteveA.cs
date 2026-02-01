using UnityEngine;
using UnityEngine.UI;

public class SteveA : MonoBehaviour
{
    [Header("Refs")]
    public AnimalController cat;          // 需要 cat.right()
    public BubbleAppear bubble;              // 需要 bubble.Appear()
    public SpriteRenderer tvRenderer;  // TV 的 SpriteRenderer（换图用）

    [Header("Option UI")]
    public GameObject optionPanel;
    public Button tvButton;
    public Button musicButton;

    [Header("TV Sprites")]
    public Sprite tvChangedSprite;

    [Header("Animator (on this GameObject)")]
    public string playingParam = "playing";
    private Animator anim;

    [Header("Sound IDs")]
    public int bootSfxId ;
    public int wrongSfxId ;

    public int tvSfxId ;
    public int tvAfterChangeSfxId ;   // 可选：0 表示不用

    public int musicIntroSfxId ;
    public int musicLoopSfxId ;       // 可选：0 表示用 intro 当 loop

    private int seq = 0;

    [SerializeField] private SpriteRenderer highlightRenderer;
    void Awake()
    {
        anim = GetComponent<Animator>();

        if (optionPanel != null) optionPanel.SetActive(false);

        if (tvButton != null)
        {
            tvButton.onClick.RemoveListener(TV);
            tvButton.onClick.AddListener(TV);
        }

        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(Music);
            musicButton.onClick.AddListener(Music);
        }
    }

    // 正确结果（由 MaskInteract 调用）
    public void Right()
    {
        int mySeq = ++seq;

        if (optionPanel != null) optionPanel.SetActive(false);
        SetPlaying(true);
        SoundManager.Instance?.PlaySound(bootSfxId, false, onCompleted: () =>
        {
            if (seq != mySeq) return;
            BootSFXFinished();
        });
    }

    // 错误结果（由 MaskInteract 调用）
    public void Wrong()
    {
        SoundManager.Instance?.PlaySound(wrongSfxId, false);
        bubble?.Appear();
    }

    // boot 播完：出现选项 + playing=true
    public void BootSFXFinished()
    {
        
        if (optionPanel != null) optionPanel.SetActive(true);
    }

    public void TV()
    {
        int mySeq = ++seq;

        if (optionPanel != null) optionPanel.SetActive(false);

        SoundManager.Instance?.PlaySound(tvSfxId, false, onCompleted: () =>
        {
            if (seq != mySeq) return;
            TVSFXFinished();
        });
    }

    // TV 分支结束：playing=false
    public void TVSFXFinished()
    {
        SetPlaying(false);

        if (tvRenderer != null && tvChangedSprite != null)
            tvRenderer.sprite = tvChangedSprite;

        if (tvAfterChangeSfxId != 0)
            SoundManager.Instance?.PlaySound(tvAfterChangeSfxId, false);

        cat?.Right();
    }

    public void Music()
    {
        int mySeq = ++seq;

        if (optionPanel != null) optionPanel.SetActive(false);

        SoundManager.Instance?.PlaySound(musicIntroSfxId, false, onCompleted: () =>
        {
            if (seq != mySeq) return;
            MusicSFXFinished();
        });
    }

    // Music 分支结束：playing=false（即使后面音乐 loop 继续播）
    public void MusicSFXFinished()
    {
        SetPlaying(false);

        int loopId = (musicLoopSfxId != 0) ? musicLoopSfxId : musicIntroSfxId;
        SoundManager.Instance?.PlaySound(loopId, true);
        SoundManager.Instance?.StopSound(308);

        TaskList.Instance?.CompleteTask(400);
    }

    private void SetPlaying(bool v)
    {
        if (anim == null) return;
        anim.SetBool(playingParam, v);
    }

     // 在Inspector里拖：要被打开的那个SpriteRenderer

    protected void OnTriggerEnter2D(Collider2D other)
    {
        

           
            if (highlightRenderer != null)
                highlightRenderer.enabled = true;
        
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
       
            if (highlightRenderer != null)
                highlightRenderer.enabled = false;

            // 如果你有这类逻辑也可以保留/按你的结构处理
  
        
    }

}
